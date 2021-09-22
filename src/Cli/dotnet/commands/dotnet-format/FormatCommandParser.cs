﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using System.CommandLine;
using System.CommandLine.Invocation;

using Microsoft.DotNet.Cli.Format;
using LocalizableStrings = Microsoft.DotNet.Tools.Format.LocalizableStrings;
using static Microsoft.DotNet.Cli.Format.FormatCommandCommon;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Cli
{
    internal static partial class FormatCommandParser
    {
        public static readonly string DocsLink = "https://aka.ms/dotnet-format";

        private static readonly FormatCommandDefaultHandler s_formatCommandHandler = new();

        private static readonly Command Command = ConstructCommand();

        public static Command GetCommand()
        {
            return Command;
        }

        private static Command ConstructCommand()
        {
            var formatCommand = new DocumentedCommand("format", DocsLink, LocalizableStrings.Formats_code_to_match_editorconfig_settings)
            {
                FormatWhitespaceCommandParser.GetCommand(),
                FormatStyleCommandParser.GetCommand(),
                FormatAnalyzersCommandParser.GetCommand(),
                DiagnosticsOption,
                SeverityOption,
            };
            formatCommand.AddCommonOptions();
            formatCommand.Handler = s_formatCommandHandler;
            return formatCommand;
        }

        class FormatCommandDefaultHandler : ICommandHandler
        {
            public Task<int> InvokeAsync(InvocationContext context)
                => Task.FromResult(new FormatCommand().FromArgs(context.ParseResult).Execute());
        }
    }
}
