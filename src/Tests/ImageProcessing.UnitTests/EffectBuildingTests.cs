using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using ImageProcessing.Commons;
using ImageProcessing.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageProcessing.UnitTests
{
    [TestClass]
    public class EffectBuildingTests
    {
        #region IEffectPipelineExecuter _executerA, _executerB, _executerC

        private IEffectPipelineExecuter _executerA = A.Fake<IEffectPipelineExecuter>();
        private IEffectPipelineExecuter _executerB = A.Fake<IEffectPipelineExecuter>();
        private IEffectPipelineExecuter _executerC = A.Fake<IEffectPipelineExecuter>();

        #endregion // IEffectPipelineExecuter _executerA, _executerB, _executerC

        #region IEffectParameters _parametersA, _parametersB, _parametersC

        private IEffectParameters _parametersA = A.Fake<IEffectParameters>();
        private IEffectParameters _parametersB = A.Fake<IEffectParameters>();
        private IEffectParameters _parametersC = A.Fake<IEffectParameters>();

        #endregion // IEffectParameters _parametersA, _parametersB, _parametersC

        private IEffectProviderFactory _providerFactory = A.Fake<IEffectProviderFactory>();

        private EffectBuilderFactory _builderFactory;

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            // arrange

            _builderFactory = new EffectBuilderFactory(_providerFactory);

            #region CanProcess

            A.CallTo(() => _providerFactory.CanProcess(_parametersA))
                    .Returns(true);
            A.CallTo(() => _providerFactory.CanProcess(_parametersB))
                    .Returns(true);
            A.CallTo(() => _providerFactory.CanProcess(_parametersC))
                    .Returns(true);

            #endregion // CanProcess

            #region CreateExecutor

            A.CallTo(() => _providerFactory.CreateExecutor(null, _parametersA))
                    .ReturnsLazily<IEffectPipelineExecuter, IEffectPipelineExecuter, IEffectParameters>(
                        (e, p) => _executerA);
            A.CallTo(() => _providerFactory.CreateExecutor(_executerA, _parametersB))
                    .ReturnsLazily<IEffectPipelineExecuter, IEffectPipelineExecuter, IEffectParameters>(
                        (e, p) => _executerB);
            A.CallTo(() => _providerFactory.CreateExecutor(_executerB, _parametersC))
                    .ReturnsLazily<IEffectPipelineExecuter, IEffectPipelineExecuter, IEffectParameters>(
                        (e, p) => _executerC);

            #endregion // CreateExecutor
            A.CallTo(() => _executerC.ExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                    .ReturnsLazily(() => Task.CompletedTask);
        }

        #endregion // Setup

        #region BasicPipeline_Test

        [TestMethod]
        public async Task BasicPipeline_Test()
        {
            // arrange 

            // act 

            IEffectPipelineExecuter execution =
                _builderFactory.Create(_parametersA)
                           .Append(_parametersB)
                           .Append(_parametersC)
                           .Build();


            await CheckBasicPipelineAsync(execution);
        }

        #endregion // BasicPipeline_Test

        #region BasicPipeline_Plus_Test

        [TestMethod]
        public async Task BasicPipeline_Plus_Test()
        {
            // arrange 

            // act 

            var builder = _builderFactory.Create(_parametersA);
            builder = builder + _parametersB + _parametersC;
            IEffectPipelineExecuter execution = builder.Build();

            await CheckBasicPipelineAsync(execution);
        }

        #endregion // BasicPipeline_Plus_Test

        #region CheckBasicPipelineAsync

        private async Task CheckBasicPipelineAsync(IEffectPipelineExecuter execution)
        {
            // act
            var srm = A.Fake<Stream>();
            await execution.ExecuteAsync(srm, srm)
                         .ConfigureAwait(false);

            // assertion

            #region CreateExecutor

            A.CallTo(() => _providerFactory.CreateExecutor(null, _parametersA))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _providerFactory.CreateExecutor(_executerA, _parametersB))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _providerFactory.CreateExecutor(_executerB, _parametersC))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _providerFactory.CreateExecutor(A<IEffectPipelineExecuter>.Ignored, A<IEffectParameters>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            #endregion // CreateExecutor

            #region ExecuteAsync

            A.CallTo(() => _executerA.ExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .MustHaveHappened(Repeated.Never);
            A.CallTo(() => _executerB.ExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .MustHaveHappened(Repeated.Never);
            A.CallTo(() => _executerC.ExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            #endregion // ExecuteAsync
        }

        #endregion // CheckBasicPipelineAsync
    }
}
