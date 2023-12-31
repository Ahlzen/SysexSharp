using System.Diagnostics;
using System.Text;
using Ahlzen.SysexSharp.SysexLib;
using Microsoft.AspNetCore.Components.Forms;

namespace SysexWebApp.Pages
{
    public partial class Index
    {
        private List<Sysex> _sysexes = new ();

        private StringBuilder _log = new();


        public async void LoadFiles(InputFileChangeEventArgs e)
        {
            Trace.WriteLine("LoadFiles");
            _log.AppendLine($"Loading file: {e.File.Name} ({e.File.Size} bytes)");

            // TODO: Limit file size

            byte[] data = null;
            try
            {
                //data = new BinaryReader(e.File.OpenReadStream()).ReadBytes((int)e.File.Size);
                MemoryStream ms = new MemoryStream();
                await e.File.OpenReadStream().CopyToAsync(ms);
                data = ms.ToArray();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //throw;
                _log.AppendLine(exception.Message);
                return;
            }

            Sysex sysex = null;
            try
            {
                sysex = SysexFactory.Create(data, e.File.Name);
                _sysexes.Add(sysex);
                _log.AppendLine($"Loaded {sysex.ManufacturerName} {sysex.Type} ({sysex.Length} bytes)");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //throw;
                _log.AppendLine(exception.Message);
                return;
            }
            StateHasChanged();
        }

    }
}
