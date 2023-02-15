// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Validation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using UnitTests.Common;
using UnitTests.Validation.Setup;
using Xunit;

namespace UnitTests.Validation;

public class DPoPProofValidatorTests
{
    private const string Category = "DPoP validator";

    private IdentityServerOptions _options = new IdentityServerOptions();
    private StubClock _clock = new StubClock();
    
    private DateTime _now = new DateTime(2020, 3, 10, 9, 0, 0, DateTimeKind.Utc);
    public DateTime UtcNow
    {
        get
        {
            if (_now > DateTime.MinValue) return _now;
            return DateTime.UtcNow;
        }
    }

    Dictionary<string, object> _header;
    Dictionary<string, object> _payload;
    string _privateJWK = "{\"Crv\":null,\"D\":\"QeBWodq0hSYjfAxxo0VZleXLqwwZZeNWvvFfES4WyItao_-OJv1wKA7zfkZxbWkpK5iRbKrl2AMJ52AtUo5JJ6QZ7IjAQlgM0lBg3ltjb1aA0gBsK5XbiXcsV8DiAnRuy6-XgjAKPR8Lo-wZl_fdPbVoAmpSdmfn_6QXXPBai5i7FiyDbQa16pI6DL-5SCj7F78QDTRiJOqn5ElNvtoJEfJBm13giRdqeriFi3pCWo7H3QBgTEWtDNk509z4w4t64B2HTXnM0xj9zLnS42l7YplJC7MRibD4nVBMtzfwtGRKLj8beuDgtW9pDlQqf7RVWX5pHQgiHAZmUi85TEbYdQ\",\"DP\":\"h2F54OMaC9qq1yqR2b55QNNaChyGtvmTHSdqZJ8lJFqvUorlz-Uocj2BTowWQnaMd8zRKMdKlSeUuSv4Z6WmjSxSsNbonI6_II5XlZLWYqFdmqDS-xCmJY32voT5Wn7OwB9xj1msDqrFPg-PqSBOh5OppjCqXqDFcNvSkQSajXc\",\"DQ\":\"VABdS20Nxkmq6JWLQj7OjRxVJuYsHrfmWJmDA7_SYtlXaPUcg-GiHGQtzdDWEeEi0dlJjv9I3FdjKGC7CGwqtVygW38DzVYJsV2EmRNJc1-j-1dRs_pK9GWR4NYm0mVz_IhS8etIf9cfRJk90xU3AL3_J6p5WNF7I5ctkLpnt8M\",\"E\":\"AQAB\",\"K\":null,\"KeyOps\":[],\"Kty\":\"RSA\",\"N\":\"yWWAOSV3Z_BW9rJEFvbZyeU-q2mJWC0l8WiHNqwVVf7qXYgm9hJC0j1aPHku_Wpl38DpK3Xu3LjWOFG9OrCqga5Pzce3DDJKI903GNqz5wphJFqweoBFKOjj1wegymvySsLoPqqDNVYTKp4nVnECZS4axZJoNt2l1S1bC8JryaNze2stjW60QT-mIAGq9konKKN3URQ12dr478m0Oh-4WWOiY4HrXoSOklFmzK-aQx1JV_SZ04eIGfSw1pZZyqTaB1BwBotiy-QA03IRxwIXQ7BSx5EaxC5uMCMbzmbvJqjt-q8Y1wyl-UQjRucgp7hkfHSE1QT3zEex2Q3NFux7SQ\",\"Oth\":null,\"P\":\"_T7MTkeOh5QyqlYCtLQ2RWf2dAJ9i3wrCx4nEDm1c1biijhtVTL7uJTLxwQIM9O2PvOi5Dq-UiGy6rhHZqf5akWTeHtaNyI-2XslQfaS3ctRgmGtRQL_VihK-R9AQtDx4eWL4h-bDJxPaxby_cVo_j2MX5AeoC1kNmcCdDf_X0M\",\"Q\":\"y5ZSThaGLjaPj8Mk2nuD8TiC-sb4aAZVh9K-W4kwaWKfDNoPcNb_dephBNMnOp9M1br6rDbyG7P-Sy_LOOsKg3Q0wHqv4hnzGaOQFeMJH4HkXYdENC7B5JG9PefbC6zwcgZWiBnsxgKpScNWuzGF8x2CC-MdsQ1bkQeTPbJklIM\",\"QI\":\"i716Vt9II_Rt6qnjsEhfE4bej52QFG9a1hSnx5PDNvRrNqR_RpTA0lO9qeXSZYGHTW_b6ZXdh_0EUwRDEDHmaxjkIcTADq6JLuDltOhZuhLUSc5NCKLAVCZlPcaSzv8-bZm57mVcIpx0KyFHxvk50___Jgx1qyzwLX03mPGUbDQ\",\"Use\":null,\"X\":null,\"X5c\":[],\"X5t\":null,\"X5tS256\":null,\"X5u\":null,\"Y\":null,\"KeySize\":2048,\"HasPrivateKey\":true,\"CryptoProviderFactory\":{\"CryptoProviderCache\":{},\"CustomCryptoProvider\":null,\"CacheSignatureProviders\":true,\"SignatureProviderObjectPoolCacheSize\":80}}";
    string _publicJWK = "{\"kty\":\"RSA\",\"use\":\"sig\",\"x5t\":null,\"e\":\"AQAB\",\"n\":\"yWWAOSV3Z_BW9rJEFvbZyeU-q2mJWC0l8WiHNqwVVf7qXYgm9hJC0j1aPHku_Wpl38DpK3Xu3LjWOFG9OrCqga5Pzce3DDJKI903GNqz5wphJFqweoBFKOjj1wegymvySsLoPqqDNVYTKp4nVnECZS4axZJoNt2l1S1bC8JryaNze2stjW60QT-mIAGq9konKKN3URQ12dr478m0Oh-4WWOiY4HrXoSOklFmzK-aQx1JV_SZ04eIGfSw1pZZyqTaB1BwBotiy-QA03IRxwIXQ7BSx5EaxC5uMCMbzmbvJqjt-q8Y1wyl-UQjRucgp7hkfHSE1QT3zEex2Q3NFux7SQ\",\"x5c\":null,\"x\":null,\"y\":null,\"crv\":null}";
    string _JKT = "JGSVlE73oKtQQI1dypYg8_JNat0xJjsQNyOI5oxaZf4";

    DefaultDPoPProofValidator _subject;

    public DPoPProofValidatorTests()
    {
        _clock.UtcNowFunc = () => UtcNow;
        _subject = new DefaultDPoPProofValidator(new MockServerUrls() { BasePath = "/", Origin = "https://identityserver" }, _clock, new LoggerFactory().CreateLogger<DefaultDPoPProofValidator>());

        _payload = new Dictionary<string, object>
        {
            { "jti", "random" },
            { "htm", "POST" },
            { "htu", "https://identityserver/connect/token" },
            { "iat", _clock.UtcNow.UtcTicks },
        };

        CreateHeaderValuesFromPublicKey();
    }

    void CreateHeaderValuesFromPublicKey()
    {
        var jwk = JsonSerializer.Deserialize<JsonElement>(_publicJWK);
        var jwkValues = new Dictionary<string, object>();
        foreach (var item in jwk.EnumerateObject())
        {
            if (item.Value.ValueKind == JsonValueKind.String)
            {
                var val = item.Value.GetString();
                if (!String.IsNullOrEmpty(val))
                {
                    jwkValues.Add(item.Name, val);
                }
            }
            if (item.Value.ValueKind == JsonValueKind.False)
            {
                jwkValues.Add(item.Name, false);
            }
            if (item.Value.ValueKind == JsonValueKind.True)
            {
                jwkValues.Add(item.Name, true);
            }
            if (item.Value.ValueKind == JsonValueKind.Number)
            {
                jwkValues.Add(item.Name, item.Value.GetInt64());
            }
        }
        _header = new Dictionary<string, object>()
        {
            //{ "alg", "RS265" }, // JsonWebTokenHandler requires adding this itself
            { "typ", "dpop+jwk" },
            { "jwk", jwkValues },
        };
    }

    string CreateDPoPProofToken(string alg = "RS256", SecurityKey key = null)
    {
        key ??= new JsonWebKey(_privateJWK);
        var handler = new JsonWebTokenHandler() { SetDefaultTimesOnTokenCreation = false };
        var token = handler.CreateToken(JsonSerializer.Serialize(_payload), new SigningCredentials(key, alg), _header);
        return token;
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task empty_string_should_fail_validation()
    {
        var popToken = "";
        var ctx = new DPoPProofValidatonContext { ProofToken = popToken };
        var result = await _subject.ValidateAsync(ctx);
        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task malformed_dpop_jwt_should_fail_validation()
    {
        var token = "malformed";
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task valid_dpop_jwt_should_pass_validation()
    {
        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeFalse();
        result.JsonWebKeyThumbprint.Should().Be(_JKT);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task invalid_typ_dpop_jwt_should_fail_validation()
    {
        _header["typ"] = "JWT";

        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task invalid_alg_dpop_jwt_should_fail_validation()
    {
        var key = new SymmetricSecurityKey(IdentityModel.CryptoRandom.CreateRandomKey(32));
        _publicJWK = JsonSerializer.Serialize(key);
        CreateHeaderValuesFromPublicKey();
        var token = CreateDPoPProofToken("HS256", key);

        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task jwk_with_private_key_dpop_jwt_should_fail_validation()
    {
        _publicJWK = _privateJWK;
        CreateHeaderValuesFromPublicKey();

        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task jwk_with_malformed_typ_dpop_jwt_should_fail_validation()
    {
        _header["typ"] = true;

        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task jwk_with_missing_jwk_dpop_jwt_should_fail_validation()
    {
        _header.Remove("jwk");

        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task jwk_with_malformed_key_dpop_jwt_should_fail_validation()
    {
        _header["jwk"] = "malformed";
        
        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task mismatched_key_dpop_jwt_should_fail_validation()
    {
        var key = CryptoHelper.CreateRsaSecurityKey();
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(key);
        _privateJWK = JsonSerializer.Serialize(jwk);

        var token = CreateDPoPProofToken();
        var ctx = new DPoPProofValidatonContext { ProofToken = token };
        var result = await _subject.ValidateAsync(ctx);
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be("invalid_dpop_proof");
    }
}