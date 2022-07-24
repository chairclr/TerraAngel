using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using Microsoft.CodeAnalysis;

namespace TerraAngel.Utility
{
    public class CSharpREPL
    {
        private static ScriptState<object>? scriptState = null;
        private static AdhocWorkspace? completionWorkspace = null;
        private static string[] defaultUsings = new string[] 
        {
            "System",
            "System.IO",
            "System.Collections.Generic",
            "System.Console", 
            "System.Diagnostics", 
            "System.Dynamic",
            "System.Linq",
            "System.Linq.Expressions", 
            "System.Net.Http",
            "System.Text",
            "System.Threading.Tasks",
            "Terraria",
            "TerraAngel",
            "TerraAngel.Client",
            "TerraAngel.Client.ClientWindows",
            "TerraAngel.Client.Config",
            "TerraAngel.Cheat",
            "Microsoft.Xna.Framework",
        };

        public static Task ExecuteAsync(string code)
        {
            return Task.Run(
                async () =>
                {
                    if (scriptState is null)
                    {
                        ScriptOptions compilationOptions = ScriptOptions.Default
                            .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => File.Exists(x.Location)))
                            .AddImports(defaultUsings);
                        scriptState = await CSharpScript.RunAsync("", compilationOptions);
                    }

                    try
                    {
                        scriptState = await scriptState.ContinueWithAsync(code, catchException: (x) => true);
                    }
                    catch (CompilationErrorException cex)
                    {
                        ClientLoader.Console.WriteError(cex.Message);
                    }
                    catch (Exception ex)
                    {
                        ClientLoader.Console.WriteError(ex.ToString());
                    }

                    if (scriptState.ReturnValue is not null) ClientLoader.Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(scriptState.ReturnValue));
                    if (scriptState.Exception is not null) ClientLoader.Console.WriteError(scriptState.Exception.ToString());
                });
        }
        public static void Execute(string code)
        {
            if (scriptState is null)
            {
                ScriptOptions compilationOptions = ScriptOptions.Default
                    .AddReferences(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && File.Exists(x.Location)))
                    .AddImports(defaultUsings);
                scriptState = CSharpScript.RunAsync("", compilationOptions).Result;
            }

            bool success = false;
            try
            {
                scriptState = scriptState.ContinueWithAsync(code, catchException: (x) => true).Result;
                success = true;
            }
            catch (CompilationErrorException cex)
            {
                ClientLoader.Console.WriteError(cex.Message);
            }
            catch (Exception ex)
            {
                ClientLoader.Console.WriteError(ex.ToString());
            }

            if (success)
            {
                if (scriptState.ReturnValue is not null) ClientLoader.Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(scriptState.ReturnValue));
                if (scriptState.Exception is not null) ClientLoader.Console.WriteError(scriptState.Exception.ToString());
            }
        }

        public static void GetCompletion(string code, int cursorPosition)
        {

        }
    }
}
