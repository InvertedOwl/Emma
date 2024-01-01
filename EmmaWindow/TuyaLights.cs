using System;
using System.Windows;
using com.clusterrr.TuyaNet;

namespace EmmaWindow
{
    public static class TuyaLights
    {
        public static async void Init()
        {
    //         var api = new TuyaApi(region: TuyaApi.Region.WesternAmerica, accessId: "8trfh9eqphrgpag5vsu9", apiSecret: "5d250c52a0a241328c37178336c5af51");
    //
    //         String req = @"{
    // ""commands"": [
    //         {
    //             ""code"": ""switch_led"",
    //             ""value"": true
    //         }
    //         ]
    //     }";
    //         
    //         var devspec = await api.RequestAsync(TuyaApi.Method.POST, $"/v1.0/devices/ebf37197f9a98a1b90noka/commands", req);
        }

        public static async void TurnOnLamp()
        {
            var api = new TuyaApi(region: TuyaApi.Region.WesternAmerica, accessId: "8trfh9eqphrgpag5vsu9", apiSecret: "5d250c52a0a241328c37178336c5af51");

            String req = @"{
    ""commands"": [
            {
                ""code"": ""switch_led"",
                ""value"": true
            }
            ]
        }";
            
            var devspec = await api.RequestAsync(TuyaApi.Method.POST, $"/v1.0/devices/ebf37197f9a98a1b90noka/commands", req);
        }
        public static async void TurnOffLamp()
        {
            var api = new TuyaApi(region: TuyaApi.Region.WesternAmerica, accessId: "8trfh9eqphrgpag5vsu9", apiSecret: "5d250c52a0a241328c37178336c5af51");

            String req = @"{
    ""commands"": [
            {
                ""code"": ""switch_led"",
                ""value"": false
            }
            ]
        }";
            
            var devspec = await api.RequestAsync(TuyaApi.Method.POST, $"/v1.0/devices/ebf37197f9a98a1b90noka/commands", req);
        }
        
        public static async void TurnLampTo(String value)
        {
            if (value == "1000")
            {
                TurnOnLamp();
            }

            var api = new TuyaApi(region: TuyaApi.Region.WesternAmerica, accessId: "8trfh9eqphrgpag5vsu9", apiSecret: "5d250c52a0a241328c37178336c5af51");

            
            String req = @"{
        ""commands"": [
            {
                ""code"": ""bright_value_v2"",
                ""value"": "+value+@"
            }
            ]
        }";
            
            var devspec = await api.RequestAsync(TuyaApi.Method.POST, $"/v1.0/devices/ebf37197f9a98a1b90noka/commands", req);
        }
        
    }
}