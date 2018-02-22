using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using ImageProcessing.Commons;
using ImageProcessing.Contracts;
using ImageProcessing.Providers;
using ImageSharp.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageProcessing.UnitTests
{
    [TestClass]
    public class EffectBuildingTests
    {
        #region IEffectPipelineExecuter _executerA, _executerB, _executerC

        private EffectPipelineExecuterBase _executerA = A.Fake<EffectPipelineExecuterBase>();
        private EffectPipelineExecuterBase _executerB = A.Fake<EffectPipelineExecuterBase>();
        private EffectPipelineExecuterBase _executerC = A.Fake<EffectPipelineExecuterBase>();
        private EffectPipelineExecuterBase _executerGrayscale = A.Fake<EffectPipelineExecuterBase>();
        private EffectPipelineExecuterBase _executerResize = A.Fake<EffectPipelineExecuterBase>();

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
            A.CallTo(() => _providerFactory.CanProcess(A<IEffectParameters>.That.Matches(e => e is GrayscaleEffectParameters)))
                    .Returns(true);
            A.CallTo(() => _providerFactory.CanProcess(A<IEffectParameters>.That.Matches(e => e is ResizeEffectParameters)))
                    .Returns(true);

            #endregion // CanProcess

            #region CreateExecutor

            A.CallTo(() => _providerFactory.CreateExecutor(null, _parametersA))
                    .ReturnsLazily<EffectPipelineExecuterBase, EffectPipelineExecuterBase, IEffectParameters>(
                        (e, p) => _executerA);
            A.CallTo(() => _providerFactory.CreateExecutor(_executerA, _parametersB))
                    .ReturnsLazily<EffectPipelineExecuterBase, EffectPipelineExecuterBase, IEffectParameters>(
                        (e, p) => _executerB);
            A.CallTo(() => _providerFactory.CreateExecutor(_executerB, _parametersC))
                    .ReturnsLazily<EffectPipelineExecuterBase, EffectPipelineExecuterBase, IEffectParameters>(
                        (e, p) => _executerC);

            A.CallTo(() => _providerFactory.CreateExecutor(A<EffectPipelineExecuterBase>.Ignored, A<IEffectParameters>.That.Matches(e => e is GrayscaleEffectParameters)))
                    .ReturnsLazily<EffectPipelineExecuterBase, EffectPipelineExecuterBase, IEffectParameters>(
                        (e, p) => _executerGrayscale);
            A.CallTo(() => _providerFactory.CreateExecutor(A<EffectPipelineExecuterBase>.Ignored, A<IEffectParameters>.That.Matches(e => e is ResizeEffectParameters)))
                    .ReturnsLazily<EffectPipelineExecuterBase, EffectPipelineExecuterBase, IEffectParameters>(
                        (e, p) => _executerResize);

            #endregion // CreateExecutor
            A.CallTo(() => _executerC.OnExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                    .ReturnsLazily(() => Task.CompletedTask);
        }

        #endregion // Setup

        #region BasicPipeline_Test

        [TestMethod]
        public async Task EffectBuilding_BasicPipeline_Test()
        {
            // arrange 

            // act 

            EffectPipelineExecuterBase execution =
                _builderFactory.Create(_parametersA)
                           .Append(_parametersB)
                           .Append(_parametersC)
                           .Build();


            await CheckBasicPipelineAsync(execution);
        }

        #endregion // BasicPipeline_Test

        #region BasicPipeline_Frendly_Test

        [TestMethod]
        public async Task EffectBuilding_BasicPipeline_Frendly_Test()
        {
            // arrange 

            // act 

            EffectPipelineExecuterBase execution =
                _builderFactory.Create()
                           .Grayscale()
                           //.Append(new GrayscaleEffectParameters())
                           //.Grayscale(GrayscaleMode.Bt709) // this one will test the provider (not the builder)
                           //.Append(new GrayscaleModeEffectParameters(GrayscaleMode.Bt709))
                           .Resize(100, 100)
                           //.Append(new ResizeEffectParameters(100, 100))
                           .Build();

            var srm = A.Fake<Stream>();
            await execution.ExecuteAsync(srm, srm)
                         .ConfigureAwait(false);

            // asserts
            A.CallTo(() => _providerFactory.CanProcess(A<IEffectParameters>.That.Matches(e => e is GrayscaleEffectParameters)))
                    .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _providerFactory.CanProcess(A<IEffectParameters>.That.Matches(e => e is ResizeEffectParameters)))
                    .MustHaveHappened(Repeated.Exactly.Once);

            Assert.AreSame(execution, _executerResize);
            A.CallTo(() => _providerFactory.CreateExecutor(A<EffectPipelineExecuterBase>.Ignored, A<IEffectParameters>.That.Matches(e => e is GrayscaleEffectParameters)))
                    .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _providerFactory.CreateExecutor(A<EffectPipelineExecuterBase>.Ignored, A<IEffectParameters>.That.Matches(e => e is ResizeEffectParameters)))
                    .MustHaveHappened(Repeated.Exactly.Once);
        }

        #endregion // EffectBuilding_BasicPipeline_Frendly_Test

        #region BasicPipeline_Plus_Test

        [TestMethod]
        public async Task EffectBuilding_BasicPipeline_Plus_Test()
        {
            // arrange 

            // act 

            var builder = _builderFactory.Create(_parametersA);
            builder = builder + _parametersB + _parametersC;
            EffectPipelineExecuterBase execution = builder.Build();

            await CheckBasicPipelineAsync(execution);
        }

        #endregion // BasicPipeline_Plus_Test

        #region CheckBasicPipelineAsync

        private async Task CheckBasicPipelineAsync(EffectPipelineExecuterBase execution)
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
            A.CallTo(() => _providerFactory.CreateExecutor(A<EffectPipelineExecuterBase>.Ignored, A<IEffectParameters>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            #endregion // CreateExecutor

            #region ExecuteAsync

            A.CallTo(() => _executerA.OnExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .MustHaveHappened(Repeated.Never);
            A.CallTo(() => _executerB.OnExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .MustHaveHappened(Repeated.Never);
            A.CallTo(() => _executerC.OnExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Once);

            #endregion // ExecuteAsync
        }

        #endregion // CheckBasicPipelineAsync
    }
}
