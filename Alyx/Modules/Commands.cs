using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alyx.Modules;

public class Commands : ModuleBase<SocketCommandContext>
{
    // The PingAsync method is the entry point of the Ping command.
    [Command("ping")]
    public async Task Ping()
    {
        await ReplyAsync("pong");
    }

    [Command("ipping")]
    public async Task PingAsync(string ip)
    {
        await ReplyAsync("Pinging " + ip + "...");
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ping",
                Arguments = "-n 4 " + ip,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        process.WaitForExit();
        await ReplyAsync(output);
    }
}
