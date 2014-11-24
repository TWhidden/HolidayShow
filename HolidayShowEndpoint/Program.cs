using System;
using System.Linq;
using System.Net;
using System.Threading;
using CommandLine;
using HolidayShowEndpoint.Entities;

namespace HolidayShowEndpoint
{
    class Program
    {
        public readonly static BroadcomPinNumber[] PinsAvailable = new[]
            {
                BroadcomPinNumber.Four,
                    BroadcomPinNumber.Fourteen,
                    BroadcomPinNumber.Fithteen,
                    BroadcomPinNumber.Seventeen,
                    BroadcomPinNumber.Eighteen,
                    BroadcomPinNumber.TwentySeven,
                    BroadcomPinNumber.TwentyTwo,
                    BroadcomPinNumber.TwentyThree,
                    BroadcomPinNumber.TwentyFour,
                    BroadcomPinNumber.TwentyFive
            };

        public static Client _client;

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<InputParams>(args);
            if (result.Errors.Any())
            {
                // Values are available here
                Console.WriteLine(result.Errors);
                return;
            }



            Console.WriteLine("Current Time is: " + DateTime.Now.ToString());

            //if (!result.Value.SkipLightInit)
            //    InitLights();
            //else
                ResetLights();
            

            _client = new Client(new IPEndPoint(IPAddress.Parse(result.Value.ServerAddress), result.Value.ServerPort), result.Value.DeviceId);

            Console.ReadLine();


        }

        private static void ResetLights()
        {
            foreach (var broadcomPinNumber in PinsAvailable)
            {
                LibGpio.Gpio.SetupChannel(broadcomPinNumber, Direction.Output);
            }

            foreach (var t in PinsAvailable)
            {
                LibGpio.Gpio.OutputValue(t, true);
            }

            Thread.Sleep(250);

            foreach (var t in PinsAvailable)
            {
                LibGpio.Gpio.OutputValue(t, false);
            }
        }

        private static void InitLights()
        {
            try
            {

                foreach (var broadcomPinNumber in PinsAvailable)
                {
                    LibGpio.Gpio.SetupChannel(broadcomPinNumber, Direction.Output);
                }

                foreach (var t in PinsAvailable)
                {
                    LibGpio.Gpio.OutputValue(t, true);
                }

                Thread.Sleep(5000);

                foreach (var t in PinsAvailable)
                {
                    LibGpio.Gpio.OutputValue(t, false);
                }

                Thread.Sleep(2000);

                // blink how many times to indicate this light ID
                for (var i = 1; i <= PinsAvailable.Count(); i++)
                {
                    for (var x = 0; x < i; x++ )
                    {
                        LibGpio.Gpio.OutputValue(PinsAvailable[(i - 1)], true);
                        Thread.Sleep(100);
                        LibGpio.Gpio.OutputValue(PinsAvailable[(i - 1)], false);

                        Thread.Sleep(150);
                    }

                    Thread.Sleep(1000);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not init lights! " + ex.Message);
            }
        }
    }
}
