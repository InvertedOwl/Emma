using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace EmmaWindow
{
    public static class EmmaActions
    {
        public static Dictionary<string, Action> actions = new Dictionary<string, Action>()
        {
            {"<Turn_Off_Lamp>", (() =>
            {
                TuyaLights.TurnOffLamp();
            })},
            
            {"<Turn_On_Lamp>", (() => {
                TuyaLights.TurnOnLamp();
            })},
            {"<Turn_Lamp_To_100%>", (() => {
                TuyaLights.TurnLampTo("1000");
            })},
            {"<Turn_Lamp_To_75%>", (() => {
                TuyaLights.TurnLampTo("750");
            })},
            {"<Turn_Lamp_To_50%>", (() => {
                TuyaLights.TurnLampTo("500");
            })},
            {"<Turn_Lamp_To_25%>", (() => {
                TuyaLights.TurnLampTo("250");
            })},
            {"<Turn_Lamp_To_12%>", (() => {
                TuyaLights.TurnLampTo("120");
            })},
            {"<Turn_On_LEDs>", (() => {
                // MessageBox.Show("Turning On LEDs");
            })},
            
            {"<Turn_Off_LEDs>", (() => {
                // MessageBox.Show("Turning On LEDs");
            })},

        };

        public static String ParseResponse(String response)
        {
            StringBuilder result = new StringBuilder(response);
        
            foreach (var actionKey in actions.Keys)
            {
                if (result.ToString().Contains(actionKey))
                {
                    actions[actionKey](); // Execute the associated action
                    result.Replace(actionKey, ""); // Remove the action from the result string
                    Console.WriteLine("Action " + actionKey);
                }
            }

            return result.ToString();
        }
    }
}