﻿// <auto-generated />
// Generated by DotMake.CommandLine.SourceGeneration v1.0.0.0
// Roslyn (Microsoft.CodeAnalysis) v4.800.23.57201
// Generation: 1

namespace TestApp.Commands
{
	public class RootWithExternalChildrenCliCommandBuilder : DotMake.CommandLine.DotMakeCommandBuilder
	{
		public RootWithExternalChildrenCliCommandBuilder()
		{
			DefinitionType = typeof(TestApp.Commands.RootWithExternalChildrenCliCommand);
			ParentDefinitionType = null;
			NameCasingConvention = DotMake.CommandLine.DotMakeCliCasingConvention.KebabCase;
			NamePrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.DoubleHyphen;
			ShortFormPrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.SingleHyphen;
			ShortFormAutoGenerate = true;
		}

		public override System.CommandLine.RootCommand Build()
		{
			// Command for 'RootWithExternalChildrenCliCommand' class
			var rootCommand = new System.CommandLine.RootCommand()
			{
				Description = "A root cli command with external children and one nested child and testing settings inheritance",
			};
			rootCommand.AddAlias("rootCmdAlias");

			var defaultClass = new TestApp.Commands.RootWithExternalChildrenCliCommand();

			// Option for 'Option1' property
			var option0 = new System.CommandLine.Option<string>("--option-1")
			{
				Description = "Description for Option1",
			};
			option0.SetDefaultValue(defaultClass.Option1);
			option0.AddAlias("-o");
			option0.AddAlias("opt1Alias");
			rootCommand.Add(option0);

			// Option for 'Option2' property
			var option1 = new System.CommandLine.Option<string>("--option-2")
			{
				Description = "Description for Option2",
			};
			System.CommandLine.OptionExtensions.FromAmong(option1, new[] {"value1", "value2", "value3"});
			option1.SetDefaultValue(defaultClass.Option2);
			option1.AddAlias("-o");
			option1.AddAlias("globalOpt2Alias");
			rootCommand.AddGlobalOption(option1);

			// Argument for 'Argument1' property
			var argument0 = new System.CommandLine.Argument<string>("argument-1")
			{
				Description = "Description for Argument1",
			};
			argument0.SetDefaultValue(defaultClass.Argument1);
			rootCommand.Add(argument0);

			// Add nested or external registered children
			foreach (var child in Children)
			{
				rootCommand.Add(child.Build());
			}

			System.CommandLine.Handler.SetHandler(rootCommand, context =>
			{
				var targetClass = new TestApp.Commands.RootWithExternalChildrenCliCommand();

				//  Set the parsed or default values for the options
				targetClass.Option1 = context.ParseResult.GetValueForOption(option0);
				targetClass.Option2 = context.ParseResult.GetValueForOption(option1);

				//  Set the parsed or default values for the arguments
				targetClass.Argument1 = context.ParseResult.GetValueForArgument(argument0);

				//  Call the command handler
				context.ExitCode = targetClass.Run();
			});

			return rootCommand;
		}

		[System.Runtime.CompilerServices.ModuleInitializerAttribute]
		public static void Initialize()
		{
			var commandBuilder = new TestApp.Commands.RootWithExternalChildrenCliCommandBuilder();

			// Register this command builder so that it can be found by the definition class
			// and it can be found by the parent definition class if it's a nested/external child.
			commandBuilder.Register();
		}

		public class Level1SubCliCommandBuilder : DotMake.CommandLine.DotMakeCommandBuilder
		{
			public Level1SubCliCommandBuilder()
			{
				DefinitionType = typeof(TestApp.Commands.RootWithExternalChildrenCliCommand.Level1SubCliCommand);
				ParentDefinitionType = typeof(TestApp.Commands.RootWithExternalChildrenCliCommand);
				NameCasingConvention = DotMake.CommandLine.DotMakeCliCasingConvention.SnakeCase;
				NamePrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.ForwardSlash;
				ShortFormPrefixConvention = DotMake.CommandLine.DotMakeCliPrefixConvention.ForwardSlash;
				ShortFormAutoGenerate = true;
			}

			public override System.CommandLine.Command Build()
			{
				// Command for 'Level1SubCliCommand' class
				var command = new System.CommandLine.Command("level_1")
				{
					Description = "A nested level 1 sub-command with custom settings, throws test exception",
				};

				var defaultClass = new TestApp.Commands.RootWithExternalChildrenCliCommand.Level1SubCliCommand();

				// Option for 'Option1' property
				var option0 = new System.CommandLine.Option<string>("/option_1")
				{
					Description = "Description for Option1",
				};
				option0.SetDefaultValue(defaultClass.Option1);
				option0.AddAlias("/o");
				command.Add(option0);

				// Argument for 'Argument1' property
				var argument0 = new System.CommandLine.Argument<string>("argument_1")
				{
					Description = "Description for Argument1",
				};
				argument0.SetDefaultValue(defaultClass.Argument1);
				command.Add(argument0);

				// Add nested or external registered children
				foreach (var child in Children)
				{
					command.Add(child.Build());
				}

				System.CommandLine.Handler.SetHandler(command, context =>
				{
					var targetClass = new TestApp.Commands.RootWithExternalChildrenCliCommand.Level1SubCliCommand();

					//  Set the parsed or default values for the options
					targetClass.Option1 = context.ParseResult.GetValueForOption(option0);

					//  Set the parsed or default values for the arguments
					targetClass.Argument1 = context.ParseResult.GetValueForArgument(argument0);

					//  Call the command handler
					targetClass.Run();
				});

				return command;
			}

			[System.Runtime.CompilerServices.ModuleInitializerAttribute]
			public static void Initialize()
			{
				var commandBuilder = new TestApp.Commands.RootWithExternalChildrenCliCommandBuilder.Level1SubCliCommandBuilder();

				// Register this command builder so that it can be found by the definition class
				// and it can be found by the parent definition class if it's a nested/external child.
				commandBuilder.Register();
			}
		}
	}
}
