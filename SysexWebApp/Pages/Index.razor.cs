using System.Diagnostics;
using Ahlzen.SysexSharp.SysexLib;
using Microsoft.AspNetCore.Components.Forms;

namespace SysexWebApp.Pages
{
    public partial class Index
    {
        private List<Sysex> _sysexes = new ();

        private string _log = "";


        public async void LoadFiles(InputFileChangeEventArgs e)
        {
            Trace.WriteLine("LoadFiles");
            _log = $"Loading file: {e.File.Name} ({e.File.Size} bytes)";

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
                _log = exception.Message;
                return;
            }

            Sysex sysex = null;
            try
            {
                sysex = SysexFactory.Create(data);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //throw;
                _log = exception.Message;
                return;
            }

            _log = $"Loaded {sysex.ManufacturerName} {sysex.Type} ({sysex.Length} bytes)";
            
            _sysexes.Add(sysex);

        }

    }
}
