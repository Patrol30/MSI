﻿using FANNCSharp;
#if FANN_FIXED
using FANNCSharp.Fixed;
using DataType = System.Int32;
#elif FANN_DOUBLE
using FANNCSharp.Double;
using DataType = System.Double;
#else
using FANNCSharp.Float;
using DataType = System.Single;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace apr
{
    class Program
    {
        static int PrintCallback( NeuralNet net, TrainingData train, uint max_epochs, uint epochs_between_reports, float desired_error, uint epochs, Object user_data)
        {
            Console.WriteLine(String.Format("Epochs     " + String.Format("{0:D}", epochs).PadLeft(8) + ". Current Error: " +
                              String.Format("{0:F}", net.MSE).PadRight(8)));
            return 0;
        }

        static float FannAbs(float value)
        {
            return (((value) > 0) ? (value) : -(value));
        }



        static void Funcapr()
        {


            Console.WriteLine("\n Test started.");

            const float learning_rate = 0.7f;
            const uint num_layers = 3;
            const uint num_input = 2;
            const uint num_hidden = 3;
            const uint num_output = 1;
            const float desired_error = 0.001f;
            const uint max_iterations = 500000;
            const uint iterations_between_reports = 10000;

            Console.WriteLine("\nCreating network.");

            using (NeuralNet net = new NeuralNet(NetworkType.LAYER, num_layers, num_input, num_hidden, num_output))
            {
                net.LearningRate = learning_rate;

                net.ActivationSteepnessHidden = 0.5F;
                net.ActivationSteepnessOutput = 0.5F;

                net.ActivationFunctionHidden = ActivationFunction.SIGMOID_SYMMETRIC;
                net.ActivationFunctionOutput = ActivationFunction.SIGMOID_SYMMETRIC;

                // Output network type and parameters
                Console.Write("\nNetworkType                         :  ");
                switch (net.NetworkType)
                {
                    case FANNCSharp.NetworkType.LAYER:
                        Console.WriteLine("LAYER");
                        break;
                    case FANNCSharp.NetworkType.SHORTCUT:
                        Console.WriteLine("SHORTCUT");
                        break;
                    default:
                        Console.WriteLine("UNKNOWN");
                        break;
                }
                net.PrintParameters();

                Console.WriteLine("\nTraining network.");

                using (TrainingData data = new TrainingData())
                {
                    if (data.ReadTrainFromFile("daneucz3.data"))
                    {
                        // Initialize and train the network with the data
                        net.InitWeights(data);

                        Console.WriteLine("Max Epochs " + String.Format("{0:D}", max_iterations).PadLeft(8) + ". Desired Error: " + String.Format("{0:F}", desired_error).PadRight(8));
                        net.SetCallback(PrintCallback, null);
                        net.TrainOnData(data, max_iterations, iterations_between_reports, desired_error);

                        Console.WriteLine("\nTesting network.");

                        for (uint i = 0; i < data.TrainDataLength; i++)
                        {
                            // Run the network on the test data
                            DataType[] calc_out = net.Run(data.Input[i]);

                            Console.WriteLine("XOR test ({0}, {1}) -> {2}, should be {3}, difference = {4}",
                                data.InputAccessor[(int)i][0].ToString("+#;-#"),
                                data.InputAccessor[(int)i][1].ToString("+#;-#"),
                                calc_out[0] == 0 ? 0.ToString() : calc_out[0].ToString("+#.#####;-#.#####"),
                                data.OutputAccessor[(int)i][0].ToString("+#;-#"),
                                FannAbs(calc_out[0] - data.Output[i][0]));
                        }

                        Console.WriteLine("\nSaving network.");

                        // Save the network in floating point and fixed point
                        net.Save("net_float.net");
                        uint decimal_point = (uint)net.SaveToFixed("net_fixed.net");
                        data.SaveTrainToFixed("net_fixed.data", decimal_point);

                        Console.WriteLine("\n Func test completed.");
                    }
                }
            }
        }


        static void Main(string[] args)
        {
            
                Funcapr();
           
            Console.ReadKey();
            


        }
    }
}
