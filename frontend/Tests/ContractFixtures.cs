using System;
using System.Collections.Generic;
using System.Text;

namespace GpuShare.Frontend.Tests
{
    /// <summary>
    /// Canonical JSON responses derived from contract/backend/openapi.yaml.
    /// These must mirror exactly what the real backend returns.
    /// If a test fails after a backend change, update this file to match the new spec.
    /// </summary>
    public static class ContractFixtures
    {
        // POST /api/users/login  →  AuthToken.yaml: { token: string }
        public const string AuthToken = """{"token":"test-jwt-token"}""";

        // GET /api/devices  →  array of Device.yaml
        public const string DeviceList = """
        [
          {
            "deviceId": 1,
            "ownerUsername": "bob",
            "name": "RTX 4090 Workstation",
            "gpuModel": "NVIDIA RTX 4090",
            "vramMb": 24576,
            "cudaCores": 16384,
            "driverVersion": "545.92",
            "pricePerHourUsdCents": 350,
            "state": "AVAILABLE"
          }
        ]
        """;

        // POST /api/devices  →  RegisterDeviceResponse
        public const string RegisterDeviceResponse = """
        {
          "deviceId": "42",
          "ownerUsername": "alice",
          "state": "Available",
          "createdAt": "2025-01-01T00:00:00Z"
        }
        """;

        // GET /api/devices/{id}/status  →  DeviceStatus.yaml
        public const string DeviceStatus = """
        {
          "deviceId": 1,
          "online": true,
          "state": "RENTED",
          "utilizationPercent": 87.3,
          "memoryUsedMb": 17694.72,
          "temperatureCelsius": 74.0,
          "lastHeartbeat": "2025-01-01T12:00:00Z"
        }
        """;

        // POST /api/orders  →  CreateOrderResponse
        public const string CreateOrderResponse = """
        {
          "orderId": 99,
          "status": "WAITING_FOR_START",
          "connectionDetails": {
            "host": "gpu1.gpushare.io",
            "port": 22,
            "protocol": "SSH",
            "connectionUrl": "ssh://gpu1.gpushare.io:22"
          },
          "totalReservedCostCents": 1400
        }
        """;
    }
}
