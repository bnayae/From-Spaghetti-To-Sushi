using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using ImageProcessing.Commons;
using ImageProcessing.Contracts;
using ImageProcessing.Providers;
using ImageSharp.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using System.Linq;

namespace ImageProcessing.UnitTests
{
    [TestClass]
    public class EffectPipelineExecuterBaseTests
    {
        #region IEffectParameters _parametersA, _parametersB, _parametersC

        private IEffectParameters _parametersA = A.Fake<IEffectParameters>();
        private IEffectParameters _parametersB = A.Fake<IEffectParameters>();
        private IEffectParameters _parametersC = A.Fake<IEffectParameters>();

        #endregion // IEffectParameters _parametersA, _parametersB, _parametersC

        private IMetricReporter _metric = A.Fake<IMetricReporter>();
        private ILogger _logger = A.Fake<ILogger>();

        private EffectPipelineExecuterBase _executer;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            // arrange
        }

        #endregion // Setup

        #region EffectPipelineExecuterBaseTests_Execute_NoOptimized_Test

        [TestMethod]
        public async Task EffectPipelineExecuterBaseTests_Execute_NoOptimized_Test()
        {
            // arrange 
            var executerA = new Executer(_parametersA, null, _metric, _logger, false);
            var executerB = new Executer(_parametersB, executerA, _metric, _logger, false);
            var executerC = new Executer(_parametersC, executerB, _metric, _logger, false);

            // act 

            var srm = A.Fake<Stream>();
            await executerC.ExecuteAsync(srm, srm);

            // Assert
            A.CallTo(() => executerA.Fake.ShouldSkipExecute(executerB))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerB.Fake.ShouldSkipExecute(executerC))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerC.Fake.ShouldSkipExecute(A<EffectPipelineExecuterBase>.Ignored))
                .MustHaveHappened(Repeated.Never);
            A.CallTo(() => executerA.Fake.OnExecuteAsync(srm, srm,
                            A<IImmutableList<EffectPipelineExecuterBase>>.That
                                    .Matches(efs => efs.Single() == executerA)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerB.Fake.OnExecuteAsync(srm, srm,
                            A<IImmutableList<EffectPipelineExecuterBase>>.That
                                    .Matches(efs => efs.Single() == executerB)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerC.Fake.OnExecuteAsync(srm, srm,
                            A<IImmutableList<EffectPipelineExecuterBase>>.That
                                    .Matches(efs => efs.Single() == executerC)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        #endregion // EffectPipelineExecuterBaseTests_Execute_NoOptimized_Test

        #region EffectPipelineExecuterBaseTests_Execute_Optimized_Test

        [TestMethod]
        public async Task EffectPipelineExecuterBaseTests_Execute_Optimized_Test()
        {
            // arrange 
            var executerA = new Executer(_parametersA, null, _metric, _logger, true);
            var executerB = new Executer(_parametersB, executerA, _metric, _logger, false);
            var executerC = new Executer(_parametersC, executerB, _metric, _logger, false);

            // act 

            var srm = A.Fake<Stream>();
            await executerC.ExecuteAsync(srm, srm);

            // Assert
            A.CallTo(() => executerA.Fake.ShouldSkipExecute(executerB))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerB.Fake.ShouldSkipExecute(executerC))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerC.Fake.ShouldSkipExecute(A<EffectPipelineExecuterBase>.Ignored))
                .MustHaveHappened(Repeated.Never);

            A.CallTo(() => executerA.Fake.OnExecuteAsync(srm, srm,
                            A<IImmutableList<EffectPipelineExecuterBase>>.Ignored))
                .MustHaveHappened(Repeated.Never);
            A.CallTo(() => executerB.Fake.OnExecuteAsync(srm, srm,
                            A<IImmutableList<EffectPipelineExecuterBase>>.That
                                    .Matches(efs => efs.SequenceEqual(new[] { executerA , executerB }))))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => executerC.Fake.OnExecuteAsync(srm, srm,
                            A<IImmutableList<EffectPipelineExecuterBase>>.That
                                    .Matches(efs => efs.Single() == executerC)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        #endregion // EffectPipelineExecuterBaseTests_Execute_Optimized_Test


        #region Executer

        private class Executer : EffectPipelineExecuterBase
        {
            private readonly bool _shouldSkip;
            public EffectPipelineExecuterBase Fake = A.Fake<EffectPipelineExecuterBase>();

            public Executer(in IEffectParameters parameters,
                        EffectPipelineExecuterBase parent,
                        IMetricReporter metric,
                        ILogger logger, 
                        bool shouldSkip)
                            : base(parameters,metric, logger, parent)
            {
                _shouldSkip = shouldSkip;
            }

            internal protected override bool ShouldSkipExecute(EffectPipelineExecuterBase next)
            {
                Fake.ShouldSkipExecute(next);
                return _shouldSkip;
            }

            protected internal override Task OnExecuteAsync(
                Stream inputStream,
                Stream outputStream,
                IImmutableList<EffectPipelineExecuterBase> effectExecuters)
            {
                Fake.OnExecuteAsync(inputStream, outputStream, effectExecuters);
                return Task.CompletedTask;
            }
        }

        #endregion // Executer
    }
}
