﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NeuralNetworkNET.APIs.Interfaces;
using NeuralNetworkNET.APIs.Interfaces.Data;
using NeuralNetworkNET.APIs.Results;
using NeuralNetworkNET.APIs.Structs;
using NeuralNetworkNET.Networks.Implementations;
using NeuralNetworkNET.SupervisedLearning.Data;
using NeuralNetworkNET.SupervisedLearning.Optimization.Parameters;
using NeuralNetworkNET.SupervisedLearning.Optimization.Progress;

namespace NeuralNetworkNET.APIs
{
    /// <summary>
    /// A <see langword="delegate"/> that represents a factory that produces instances of a specific layer type, with user-defined parameters.
    /// This wrapper acts as an intemediary to streamline the user-side C# sintax when building up a new network structure, as all the input
    /// details for each layer will be automatically computed during the network setup.
    /// </summary>
    /// <param name="info">The <see cref="TensorInfo"/> for the inputs of the upcoming network layer</param>
    /// <remarks>It is also possible to invoke a <see cref="LayerFactory"/> instance just like any other <see langword="delegate"/> to immediately get an <see cref="INetworkLayer"/> value</remarks>
    [NotNull]
    public delegate INetworkLayer LayerFactory(TensorInfo info);

    /// <summary>
    /// A static class that create and trains a neural network for the input data and expected results
    /// </summary>
    public static class NetworkManager
    {
        /// <summary>
        /// Creates a new network with a linear structure and the specified parameters
        /// </summary>
        /// <param name="input">The input <see cref="TensorInfo"/> description</param>
        /// <param name="factories">A list of factories to create the different layers in the new network</param>
        [PublicAPI]
        [Pure, NotNull]
        public static INeuralNetwork NewSequential(TensorInfo input, [NotNull, ItemNotNull] params LayerFactory[] factories)
        {
            IEnumerable<INetworkLayer> BuildLayers()
            {
                foreach (LayerFactory f in factories)
                {
                    INetworkLayer layer = f(input);
                    yield return layer;
                    input = layer.OutputInfo;
                }
            }
            return new NeuralNetwork(BuildLayers().ToArray());
        }

        #region Training

        /// <summary>
        /// Trains a neural network with the given parameters
        /// </summary>
        /// <param name="network">The existing <see cref="INeuralNetwork"/> to train with the given dataset(s)</param>
        /// <param name="dataset">The <see cref="ITrainingDataset"/> instance to use to train the network</param>
        /// <param name="algorithm">The desired training algorithm to use</param>
        /// <param name="epochs">The number of epochs to run with the training data</param>
        /// <param name="dropout">Indicates the dropout probability for neurons in a <see cref="Enums.LayerType.FullyConnected"/> layer</param>
        /// <param name="batchProgress">An optional callback to monitor the training progress (in terms of dataset completion)</param>
        /// <param name="trainingProgress">An optional progress callback to monitor progress on the training dataset (in terms of classification performance)</param>
        /// <param name="validationDataset">An optional dataset used to check for convergence and avoid overfitting</param>
        /// <param name="testDataset">The optional test dataset to use to monitor the current generalized training progress</param>       
        /// <param name="token">The <see cref="CancellationToken"/> for the training session</param>
        [PublicAPI]
        [NotNull]
        [CollectionAccess(CollectionAccessType.Read)]
        public static TrainingSessionResult TrainNetwork(
            [NotNull] INeuralNetwork network,
            [NotNull] ITrainingDataset dataset,
            [NotNull] ITrainingAlgorithmInfo algorithm,
            int epochs, float dropout = 0,
            [CanBeNull] IProgress<BatchProgress> batchProgress = null,
            [CanBeNull] IProgress<TrainingProgressEventArgs> trainingProgress = null,
            [CanBeNull] IValidationDataset validationDataset = null,
            [CanBeNull] ITestDataset testDataset = null,
            CancellationToken token = default)
        {
            // Preliminary checks
            if (dropout < 0 || dropout >= 1) throw new ArgumentOutOfRangeException(nameof(dropout), "The dropout probability is invalid");

            // Start the training
            return NetworkTrainer.TrainNetwork(
                network as NeuralNetwork ?? throw new ArgumentException("The input network instance isn't valid", nameof(network)), 
                dataset as BatchesCollection ?? throw new ArgumentException("The input dataset instance isn't valid", nameof(dataset)),
                epochs, dropout, algorithm, batchProgress, trainingProgress, 
                validationDataset as ValidationDataset,
                testDataset as TestDataset,
                token);
        }

        /// <summary>
        /// Trains a neural network with the given parameters
        /// </summary>
        /// <param name="network">The existing <see cref="INeuralNetwork"/> to train with the given dataset(s)</param>
        /// <param name="dataset">The <see cref="ITrainingDataset"/> instance to use to train the network</param>
        /// <param name="algorithm">The desired training algorithm to use</param>
        /// <param name="epochs">The number of epochs to run with the training data</param>
        /// <param name="dropout">Indicates the dropout probability for neurons in a <see cref="Enums.LayerType.FullyConnected"/> layer</param>
        /// <param name="batchProgress">An optional callback to monitor the training progress (in terms of dataset completion)</param>
        /// <param name="trainingProgress">An optional progress callback to monitor progress on the training dataset (in terms of classification performance)</param>
        /// <param name="validationDataset">An optional dataset used to check for convergence and avoid overfitting</param>
        /// <param name="testDataset">The optional test dataset to use to monitor the current generalized training progress</param>       
        /// <param name="token">The <see cref="CancellationToken"/> for the training session</param>
        [PublicAPI]
        [NotNull, ItemNotNull]
        [CollectionAccess(CollectionAccessType.Read)]
        public static Task<TrainingSessionResult> TrainNetworkAsync(
            [NotNull] INeuralNetwork network,
            [NotNull] ITrainingDataset dataset,
            [NotNull] ITrainingAlgorithmInfo algorithm,
            int epochs, float dropout = 0,
            [CanBeNull] IProgress<BatchProgress> batchProgress = null,
            [CanBeNull] IProgress<TrainingProgressEventArgs> trainingProgress = null,
            [CanBeNull] IValidationDataset validationDataset = null,
            [CanBeNull] ITestDataset testDataset = null,
            CancellationToken token = default)
        {
            return Task.Run(() => TrainNetwork(network, dataset, algorithm, epochs, dropout, batchProgress, trainingProgress, validationDataset, testDataset, token), token);
        }

        #endregion

        #region Settings

        private static int _MaximumBatchSize = int.MaxValue;

        /// <summary>
        /// Gets or sets the maximum batch size (used to optimize the memory usage during validation/test processing)
        /// </summary>
        /// <remarks>Adjust this setting to the highest possible value according to the available RAM/VRAM and the size of the dataset. If the validation/test dataset has more
        /// samples than <see cref="MaximumBatchSize"/>, it will be automatically divided into batches so that it won't cause an <see cref="OutOfMemoryException"/> or other problems</remarks>
        public static int MaximumBatchSize
        {
            get => _MaximumBatchSize;
            set => _MaximumBatchSize = value >= 10 ? value : throw new ArgumentOutOfRangeException(nameof(MaximumBatchSize), "The maximum batch size must be at least equal to 10");
        }

        #endregion
    }
}