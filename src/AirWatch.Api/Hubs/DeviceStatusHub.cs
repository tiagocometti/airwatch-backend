using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AirWatch.Api.Hubs;

[Authorize]
public class DeviceStatusHub : Hub { }
