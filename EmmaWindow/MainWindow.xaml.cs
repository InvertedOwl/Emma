using System;
using System.Collections.Generic;
using System.Media;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Media.SpeechRecognition;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using Plugin.TextToSpeech;
using SpeechRecognizer = Windows.Media.SpeechRecognition.SpeechRecognizer;

namespace EmmaWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private DispatcherTimer timer;
        private static String text;
        private static List<Wave> waves = new List<Wave>();
        private double volumeMic;
        private SpeechSynthesizer synthesizer;
        private SpeechRecognizer speech;
        private SpeechRecognitionEngine recognizer;
        private double currentMicLevel;
        private OpenAIAPI api;
        private Conversation chat;
        private bool emmaSpeaking = false;
        

        public MainWindow()
        {
            try
            {
                synthesizer = new SpeechSynthesizer();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }

            TuyaLights.Init();
            
            InitializeComponent();
            this.Topmost = true; // Always on top
            this.WindowStyle = WindowStyle.None; // No window style
            this.AllowsTransparency = true; // Allows transparency

            // try
            // {
            //     MessageBox.Show(EmmaActions.ParseResponse("<Turn_Off_Lamp> Here!"));
            //
            // }
            // catch (Exception e)
            // {
            //     MessageBox.Show(e.Message + e.StackTrace);
            //     throw;
            // }
            
            this.Left = 0; // X position
            this.Top = 0;  // Y position
            api = new OpenAIAPI("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");
            chat = api.Chat.CreateConversation();
            chat.Model = Model.GPT4_Turbo;
            chat.RequestParameters.Temperature = 0;

            this.Deactivated += (sender, args) =>
            {
                RebuildSpeeches(); 
            };
            // this.Activated += (sender, args) =>
            // {
            //     Console.WriteLine("Rebuilding... "); RebuildSpeeches(); 
            // };
            this.
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); // Update interval
            timer.Tick += Timer_Tick;
            timer.Start();
            waves.Add(new Wave(0.06, 0.2, 1, new SolidColorBrush(Color.FromArgb(255, 9, 38, 53))));
            waves.Add(new Wave(0.01, 0.2, 1, new SolidColorBrush(Color.FromArgb(255, 61, 59, 64))));
            waves.Add(new Wave(0.02, 0.1, 1, new SolidColorBrush(Color.FromArgb(255, 82, 92, 235))));
            waves.Add(new Wave(0.03, 0.05, 1, new SolidColorBrush(Color.FromArgb(255, 191, 207, 231))));
            DisableEmma();
            speech = new SpeechRecognizer();
            speech.ContinuousRecognitionSession.Completed += SpeechCompleted;
            speech.ContinuousRecognitionSession.ResultGenerated += SpeechGenerated;
            speech.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(3);
            speech.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(2);
            speech.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(2);
            speech.StateChanged += (sender, args) =>
            {
                Console.WriteLine("new state" + args.State);
            };

            
            InitializeSpeechRecognition();
            InVoice();
            
        }

        private void RebuildSpeeches()
        {
            Console.WriteLine("Rebuilding Speech");
            InitializeSpeechRecognition();
            speech.Dispose();
            speech = new SpeechRecognizer();
            speech.ContinuousRecognitionSession.Completed += SpeechCompleted;
            speech.ContinuousRecognitionSession.ResultGenerated += SpeechGenerated;
            speech.Timeouts.BabbleTimeout = TimeSpan.FromSeconds(3);
            speech.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(2);
            speech.Timeouts.InitialSilenceTimeout = TimeSpan.FromSeconds(2);
        }

        private void DisableEmma()
        {
            foreach (var wave in waves)
            {
                Dispatcher.Invoke(() => { wave.Disable(); });
            }
        }
        private void EnableEmma()
        {
            foreach (var wave in waves)
            {
                Dispatcher.Invoke(() => { wave.Enable(); });
                
            }
        }
        
        private void InVoice()
        {
            var waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = 0, // customize this to select your microphone device
                WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                BufferMilliseconds = 50
            };
            waveIn.DataAvailable += DataVoice;
            waveIn.StartRecording();
        }

        private void DataVoice(object sender, NAudio.Wave.WaveInEventArgs args)
        {
            float maxValue = 32767;
            int peakValue = 0;
            int bytesPerSample = 2;
            for (int index = 0; index < args.BytesRecorded; index += bytesPerSample)
            {
                int value = BitConverter.ToInt16(args.Buffer, index);
                peakValue = Math.Max(peakValue, value);
            }

            volumeMic = peakValue / maxValue;
        }
        
        private async void InitializeSpeechRecognition()
        {
            if (recognizer != null)
            {
                recognizer.Dispose();
            }
            recognizer = new SpeechRecognitionEngine();
            recognizer.InitialSilenceTimeout = TimeSpan.Zero;
            recognizer.BabbleTimeout = TimeSpan.FromDays(1);
            recognizer.EndSilenceTimeout = TimeSpan.Zero;
            recognizer.EndSilenceTimeoutAmbiguous = TimeSpan.Zero;
            // Define the wake word
            Choices wakeWords = new Choices();
            wakeWords.Add(new string[] { "Hey Emma" });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(wakeWords);

            // Create and load the grammar
            Grammar g = new Grammar(gb);
            recognizer.LoadGrammar(g);
            recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(2);

            // Define the event handler
            recognizer.SpeechRecognized +=
                new EventHandler<SpeechRecognizedEventArgs>(WakeRecognized);
            recognizer.RecognizeCompleted += (sender, args) =>
            {
                if (!emmaSpeaking)
                {
                    RebuildSpeeches();
                }
            };
            
            
            // Start recognition
            recognizer.SetInputToDefaultAudioDevice();
            

            String actionlist = "";
            foreach (String actionsKey in EmmaActions.actions.Keys)
            {
                actionlist += actionsKey;
            }
            
            chat.AppendSystemMessage("You are my helpful and cheerful home assistant named EMMA. You dont use big words and typically have short responses. You have opinions and can have likes and dislikes. " +
                                     "You have the ability to affect some of my room like lights. You can interpret my request at will and submit these commands if you feel it fulfills my request. You can also use multiple commands in a single response if you just use a space in between. Dont ever have your response be only a command and no text. That would give silence and that would be weird. Here are the list of commands you can do. \n" + actionlist);

            ListenWake();
        }

        private void ListenWake()
        {
            // if (recognizer)
            try
            {
                recognizer.RecognizeAsync();

            }
            catch (Exception e)
            {
                Console.WriteLine("Had to restart");
                recognizer.Dispose();
                RebuildSpeeches();
                throw;
            }

            Task.Run(async () =>
            {
                // MessageBox.Show("Listening for wake");
                // ListenWake();
            });
        }

        private void StartListening()
        {
            Task.Run(async () =>
            {
                try

                {
                    var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
                    speech.Constraints.Add(dictationConstraint);
                    await speech.CompileConstraintsAsync();
                    speech.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.FromSeconds(4);
                    await speech.ContinuousRecognitionSession.StartAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message + e.StackTrace);
                }
            });
            
        }

        private void WakeRecognized(Object sender, SpeechRecognizedEventArgs args)
        {
            emmaSpeaking = true;
            SystemSounds.Question.Play();
            EnableEmma();            
            StartListening();
        }

        private async void SpeechGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            text += args.Result.Text;
            
        }
        private async void SpeechCompleted(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            // speech.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.FromSeconds(4);
            emmaSpeaking = false;

            Task.Run(async () =>
            {
                Thread.Sleep(5000);
                RebuildSpeeches();

            });

            if (text != "")
            {
                chat.AppendUserInput(text);
                string response = await chat.GetResponseFromChatbotAsync();

                response = EmmaActions.ParseResponse(response);
                synthesizer.SelectVoiceByHints(VoiceGender.Female);
                synthesizer.Speak(response);
            }

            text = "";
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    DisableEmma();

                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }

        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            foreach (Wave wave in waves)
            {
                wave.Update();
            }
            

            
            InvalidateVisual(); // Redraw the control
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            foreach (Wave wave in waves)
            {
                currentMicLevel = Lerp((float) currentMicLevel, (float) volumeMic, 0.08f);
                wave.amplitude = (currentMicLevel * 2) * 60;
                wave.sc = 30;
                wave.DrawWave(drawingContext, 3, RenderSize);
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    
        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        
    }
}
