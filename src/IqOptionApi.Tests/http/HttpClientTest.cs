﻿using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using IqOptionApi.Extensions;
using IqOptionApi.http;
using Moq;
using NUnit.Framework;
using RestSharp;
using TestAutoFixture;

namespace IqOptionApi.Tests.http {
    [TestFixture]
    public class IqOptionApiTest : TestAutoFixtureFor<IqHttpClient> {
        private Mock<IRestClient> MoqAuthClient { get; set; }
        private Mock<IRestClient> MoqHttpClient { get; set; }

        [SetUp]
        public void SetUp() {
            MoqAuthClient = InjectMock<IRestClient>();
            MoqHttpClient = InjectMock<IRestClient>();

            Fixture.Customize<IqHttpClient>(
                cfg => cfg.FromFactory(() => {
                    var cut = new IqHttpClient("", "") {
                        AuthHttpClient = MoqAuthClient.Object,
                        HttpClient = MoqHttpClient.Object
                    };

                    return cut;
                }));

            Fixture.Customize<RestResponse>(cfg =>
                cfg.With(x => x.Content, A<IqHttpResult<SsidResultMessage>>().AsJson()));
        }

        #region [LoginAsync]

        [Test]
        public async Task LoginAsync_WhenStatusCodeIsOk_IsSuccessFulMustBeTrue() {
            // arrange
            Fixture.Customize<RestResponse>(cfg =>
                cfg.With(x => x.Content, A<IqHttpResult<SsidResultMessage>>().AsJson())
                    .With(x => x.StatusCode, HttpStatusCode.OK));

            MoqAuthClient
                .Setup(x => x.ExecuteTaskAsync(Any<IRestRequest>()))
                .ReturnsAsync(A<RestResponse>());

            // act
            var instance = await CreateCut().LoginAsync();

            // assert
            instance.IsSuccessful.Should().BeTrue();
        }

        [Test]
        public async Task LoginAsync_WhenStatusCodeIsOk_CookieWasAdded() {
            // arrange
            Fixture.Customize<RestResponse>(cfg =>
                cfg.With(x => x.Content, A<IqHttpResult<SsidResultMessage>>().AsJson())
                    .With(x => x.StatusCode, HttpStatusCode.OK));

            MoqAuthClient
                .Setup(x => x.ExecuteTaskAsync(Any<IRestRequest>()))
                .ReturnsAsync(A<RestResponse>());

            // act
            var result = await CreateCut().LoginAsync();

            // assert
            result.Data.Ssid.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task LoginAsync_VerifyAuthClient_MustBeReceived() {
            // arrange
            MoqAuthClient
                .Setup(x => x.ExecuteTaskAsync(Any<IRestRequest>()))
                .ReturnsAsync(A<RestResponse>());
            // act
            var result = await CreateCut().LoginAsync();

            // assert
            MoqAuthClient.Verify(x => x.ExecuteTaskAsync(It.Is<RestRequest>(r =>
                r.Method == Method.POST &&
                r.Parameters.Any(p => p.Name == "email") &&
                r.Parameters.Any(p => p.Name == "password"))));
        }


        [Test]
        public async Task LoginAsync_WhenStatusCodeIsNotOk_IsSuccessMustBeFalse() {
            // arrange
            Fixture.Customize<RestResponse>(cfg =>
                cfg.With(x => x.Content, A<IqHttpResult<SsidResultMessage>>().AsJson())
                    .With(x => x.StatusCode, HttpStatusCode.Unauthorized));

            MoqAuthClient
                .Setup(x => x.ExecuteTaskAsync(Any<IRestRequest>()))
                .ReturnsAsync(A<RestResponse>());

            // act
            var result = await CreateCut().LoginAsync();

            // assert
            result.IsSuccessful.Should().BeFalse();
        }

        #endregion

        #region GetProfileAsync

        #endregion
    }
}