using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using GpuShare.Frontend.Models;
using GpuShare.Frontend.Models.Dtos;
using FluentAssertions;

namespace GpuShare.Frontend.Tests
{
    /// <summary>
    /// Verifies that frontend DTOs correctly map to the JSON shapes
    /// defined in contract/backend/openapi.yaml.
    /// 
    /// These tests catch mismatches between the OpenAPI spec and C# models
    /// without needing a running backend.
    /// </summary>
    public class ContractShapeTests
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        // ── Auth ────────────────────────────────────────────────────────────

        [Fact]
        public void AuthToken_Response_Deserializes_To_TokenResponse()
        {
            // ContractFixtures.AuthToken = {"token":"test-jwt-token"}
            var result = JsonSerializer.Deserialize<TokenResponse>(
                ContractFixtures.AuthToken, Options);

            result.Should().NotBeNull();
            result!.Token.Should().Be("test-jwt-token");
        }

        [Fact]
        public void AuthRequest_Serializes_To_CamelCase()
        {
            // Backend requires lowercase: {"username":...,"password":...}
            var request = new AuthRequest { Username = "alice", Password = "secret" };

            var json = JsonSerializer.Serialize(request, Options);

            json.Should().Contain("\"username\"");
            json.Should().Contain("\"password\"");
            json.Should().NotContain("\"Username\"");
            json.Should().NotContain("\"Password\"");
        }

        // ── Devices ─────────────────────────────────────────────────────────

        [Fact]
        public void Device_List_Response_Deserializes_Correctly()
        {
            var result = JsonSerializer.Deserialize<List<Device>>(
                ContractFixtures.DeviceList, Options);

            result.Should().HaveCount(1);
            var device = result![0];
            device.DeviceId.Should().Be(1);
            device.GpuModel.Should().Be("NVIDIA RTX 4090");
            device.VramMb.Should().Be(24576);
            device.State.Should().Be(DeviceState.AVAILABLE);
        }

        [Fact]
        public void RegisterDevice_Request_Contains_All_Required_Fields()
        {
            // Backend requires: name, gpuModel, vramMb, cudaCores, pricePerHourUsdCents, driverVersion
            var request = new RegisterDeviceRequest
            {
                Name = "My GPU",
                GpuModel = "RTX 4090",
                VramMb = 24576,
                CudaCores = 16384,
                PricePerHourUsdCents = 350,
                DriverVersion = "545.92"
            };

            var json = JsonSerializer.Serialize(request, Options);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // These field names must match the OpenAPI spec exactly
            root.TryGetProperty("name", out _).Should().BeTrue();
            root.TryGetProperty("gpuModel", out _).Should().BeTrue();
            root.TryGetProperty("vramMb", out _).Should().BeTrue();
            root.TryGetProperty("cudaCores", out _).Should().BeTrue();
            root.TryGetProperty("pricePerHourUsdCents", out _).Should().BeTrue();
            root.TryGetProperty("driverVersion", out _).Should().BeTrue();
        }

        // ── Orders ───────────────────────────────────────────────────────────

        [Fact]
        public void CreateOrder_Response_Deserializes_Connection_Details()
        {
            var result = JsonSerializer.Deserialize<CreateOrderResponse>(
                ContractFixtures.CreateOrderResponse, Options);

            result.Should().NotBeNull();
            result!.OrderId.Should().Be(99);
            result.ConnectionDetails.Host.Should().Be("gpu1.gpushare.io");
            result.ConnectionDetails.Port.Should().Be(22);
        }
    }
}
