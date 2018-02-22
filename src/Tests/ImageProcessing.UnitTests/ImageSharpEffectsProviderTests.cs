using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using ImageProcessing.Contracts;
using ImageProcessing.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using Setting.Contracts;

namespace ImageProcessing.UnitTests
{
    [TestClass]
    public class ImageSharpEffectsProviderTests
    {
        private ISetting _setting = A.Fake<ISetting>();
        private ILogger _logger = A.Fake<ILogger>();
        private IMetricReporter _metric = A.Fake<IMetricReporter>();

        private EffectPipelineExecuterBase _firstFakeExecuter = A.Fake<EffectPipelineExecuterBase>();
        private EffectPipelineExecuterBase _executerA;
        private EffectPipelineExecuterBase _executerB;
        private ResizeEffectParameters _resizeParameters = new ResizeEffectParameters(50, 60);

        #region Setup

        [TestInitialize]
        public void Setup()
        {
            // arrange
            A.CallTo(() => _firstFakeExecuter.OnExecuteAsync(A<Stream>.Ignored, A<Stream>.Ignored))
                .ReturnsLazily<Task, Stream, Stream>((input, output) =>
                                                    input.CopyToAsync(output));
        }

        #endregion // Setup

        #region ImageSharpEffectsProvider_Optimized_FirstFake_Test

        [TestMethod]
        public async Task ImageSharpEffectsProvider_Optimized_FirstFake_Test()
        {
            var providerSetting = new ProviderSetting(true);
            await TestExecuteAsync(providerSetting, true);
        }

        #endregion // ImageSharpEffectsProvider_Optimized_FirstFake_Test

        #region ImageSharpEffectsProvider_OptimizedTest_NoFirstFake_Test

        [TestMethod]
        public async Task ImageSharpEffectsProvider_OptimizedTest_NoFirstFake_Test()
        {
            var providerSetting = new ProviderSetting(true);
            await TestExecuteAsync(providerSetting, false);
        }

        #endregion // ImageSharpEffectsProvider_OptimizedTest_NoFirstFake_Test

        #region ImageSharpEffectsProvider_NonOptimized_FirstFake_Test

        [TestMethod]
        public async Task ImageSharpEffectsProvider_NonOptimized_FirstFake_Test()
        {
            // arrange 
            var providerSetting = new ProviderSetting(false);
            await TestExecuteAsync(providerSetting, true);
        }

        #endregion // ImageSharpEffectsProvider_NonOptimized_FirstFake_Test

        #region ImageSharpEffectsProvider_NonOptimized_NoFirstFake_Test

        [TestMethod]
        public async Task ImageSharpEffectsProvider_NonOptimized_NoFirstFake_Test()
        {
            // arrange 
            var providerSetting = new ProviderSetting(false);
            await TestExecuteAsync(providerSetting, false);
        }

        #endregion // ImageSharpEffectsProvider_NonOptimized_NoFirstFake_Test

        #region InitProvider

        private ImageSharpEffectsProvider InitProvider(bool withFirstFake = true)
        {
            var effectProvider = new ImageSharpEffectsProvider(_setting, _metric, _logger);
            EffectPipelineExecuterBase first = withFirstFake ? _firstFakeExecuter : null;
            _executerA = effectProvider.CreateExecutor(first, _resizeParameters);
            _executerB = effectProvider.CreateExecutor(_executerA, GrayscaleEffectParameters.Default);
            return effectProvider;
        }

        #endregion // InitProvider

        #region TestExecuteAsync

        private async Task TestExecuteAsync(ProviderSetting providerSetting, bool withFirstFake)
        { 
            A.CallTo(() => _setting.GetAsync<ProviderSetting>(ProviderSetting.Key))
                .ReturnsLazily(() => Task.FromResult(providerSetting));
            ImageSharpEffectsProvider effectProvider = InitProvider(withFirstFake);

            // act 
            using (var bmp = new Bitmap(100, 100))
            {
                using (var graphics = Graphics.FromImage(bmp))
                {
                    graphics.FillRectangle(Brushes.Red, 0, 0, 100, 100); // outer red rect
                    graphics.FillRectangle(Brushes.Green, 10, 10, 80, 80); // inner green rect
                }

                using (var inputStream = new MemoryStream())
                using (var outputStream = new MemoryStream())
                {
                    bmp.Save(inputStream, ImageFormat.Png);
                    inputStream.Position = 0;
                    await _executerB.ExecuteAsync(inputStream, outputStream);

                    Validation(inputStream, outputStream, withFirstFake);
                }
            }
            // assert
        }

        #endregion // TestExecuteAsync

        #region Validation

        private void Validation(
            MemoryStream inputStream,
            MemoryStream outputStream,
            bool withFirstFake)
        {
            inputStream.Position = 0;
            using (var affectedImage = Image.FromStream(outputStream))
            using (var affectedBitmap = new Bitmap(affectedImage))
            {
                // assert
                if (withFirstFake)
                {
                    A.CallTo(() => _firstFakeExecuter.OnExecuteAsync(inputStream, A<Stream>.Ignored))
                        .MustHaveHappened(Repeated.Exactly.Once);
                }
                else
                {
                    A.CallTo(() => _firstFakeExecuter.OnExecuteAsync(inputStream, A<Stream>.Ignored))
                        .MustHaveHappened(Repeated.Never);
                }
                Assert.AreEqual(_resizeParameters.Width, affectedImage.Width);
                Assert.AreEqual(_resizeParameters.Height, affectedImage.Height);
                Assert.IsTrue(affectedBitmap.GetPixel(1, 1).R < Color.Red.R);
                Assert.IsTrue(affectedBitmap.GetPixel(15, 15).G < Color.Green.G);
                Assert.IsTrue(affectedBitmap.GetPixel(1, 1) == affectedBitmap.GetPixel(2, 2));
                Assert.IsTrue(affectedBitmap.GetPixel(1, 1) != affectedBitmap.GetPixel(9, 9));
            }
        }

        #endregion // Validation
    }
}
