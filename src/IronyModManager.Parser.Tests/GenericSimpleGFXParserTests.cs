﻿// ***********************************************************************
// Assembly         : IronyModManager.Parser.Tests
// Author           : Mario
// Created          : 02-18-2020
//
// Last Modified By : Mario
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="GenericSimpleGFXParserTests.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using IronyModManager.Parser.Common.Args;
using IronyModManager.Parser.Generic;
using IronyModManager.Shared;
using IronyModManager.Tests.Common;
using Xunit;

namespace IronyModManager.Parser.Tests
{
    /// <summary>
    /// Class GenericGraphicsParserTests.
    /// </summary>
    public class GenericSimpleGFXParserTests
    {
        /// <summary>
        /// Defines the test method CanParse_should_be_false_then_true.
        /// </summary>
        [Fact]
        public void CanParse_should_be_false_then_true()
        {
            var lines = new HashSet<string>();
            for (int i = 0; i < 21000; i++)
            {
                lines.Add(i.ToString());
            }
            var args = new CanParseArgs()
            {
                File = "common\\gamerules\\test.txt",
                Lines = lines
            };
            var parser = new SimpleGFXParser(new CodeParser(), null);
            parser.CanParse(args).Should().BeFalse();
            args.File = "gfx\\gfx.gfx";
            parser.CanParse(args).Should().BeTrue();

        }

        /// <summary>
        /// Defines the test method Parse_should_yield_results.
        /// </summary>
        [Fact]
        public void Parse_should_yield_results()
        {
            DISetup.SetupContainer();

            var sb = new StringBuilder();
            sb.AppendLine(@"bitmapfonts = {");
            sb.AppendLine(@"");
            sb.AppendLine(@"	### ship size icons ###");
            sb.AppendLine(@"");
            sb.AppendLine(@"	bitmapfont = {");
            sb.AppendLine(@"		name = ""GFX_text_military_size_1""");
            sb.AppendLine(@"		texturefile = ""gfx/interface/icons/text_icons/icon_text_military_size_1.dds""");
            sb.AppendLine(@"	}");
            sb.AppendLine(@"	");
            sb.AppendLine(@"	bitmapfont = {");
            sb.AppendLine(@"		name = ""GFX_text_military_size_2""");
            sb.AppendLine(@"		texturefile = ""gfx/interface/icons/text_icons/icon_text_military_size_2.dds""");
            sb.AppendLine(@"	}");
            sb.AppendLine(@"}");


            var sb2 = new StringBuilder();
            sb2.AppendLine(@"bitmapfonts = {");
            sb2.AppendLine(@"	bitmapfont = {");
            sb2.AppendLine(@"		name = ""GFX_text_military_size_1""");
            sb2.AppendLine(@"		texturefile = ""gfx/interface/icons/text_icons/icon_text_military_size_1.dds""");
            sb2.AppendLine(@"	}");
            sb2.AppendLine(@"}");


            var sb3 = new System.Text.StringBuilder();
            sb3.AppendLine(@"bitmapfonts = {");
            sb3.AppendLine(@"	bitmapfont = {");
            sb3.AppendLine(@"		name = ""GFX_text_military_size_2""");
            sb3.AppendLine(@"		texturefile = ""gfx/interface/icons/text_icons/icon_text_military_size_2.dds""");
            sb3.AppendLine(@"	}");
            sb3.AppendLine(@"}");


            var args = new ParserArgs()
            {
                ContentSHA = "sha",
                ModDependencies = new List<string> { "1" },
                File = "gfx\\gfx.gfx",
                Lines = sb.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                ModName = "fake"
            };
            var parser = new SimpleGFXParser(new CodeParser(), null);
            var result = parser.Parse(args).ToList();
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);
            for (int i = 0; i < 2; i++)
            {
                result[i].ContentSHA.Should().Be("sha");
                result[i].Dependencies.First().Should().Be("1");
                result[i].File.Should().Be("gfx\\gfx.gfx");
                switch (i)
                {

                    case 0:
                        result[i].Id.Should().Be("GFX_text_military_size_1");
                        result[i].Code.Should().Be(sb2.ToString().Trim().ReplaceTabs());
                        result[i].ValueType.Should().Be(Common.ValueType.Object);
                        break;
                    case 1:
                        result[i].Id.Should().Be("GFX_text_military_size_2");
                        result[i].Code.Should().Be(sb3.ToString().Trim().ReplaceTabs());
                        result[i].ValueType.Should().Be(Common.ValueType.Object);
                        break;
                    default:
                        break;
                }
                result[i].ModName.Should().Be("fake");
                result[i].Type.Should().Be("gfx\\gfx");
            }
        }

        /// <summary>
        /// Defines the test method Parse_ending_edge_case_should_yield_results.
        /// </summary>
        [Fact]
        public void Parse_ending_edge_case_should_yield_results()
        {
            DISetup.SetupContainer();

            var sb = new StringBuilder();
            sb.AppendLine(@"bitmapfonts = {");
            sb.AppendLine(@"	bitmapfont = {");
            sb.AppendLine(@"		name = ""GFX_text_military_size_1""");
            sb.AppendLine(@"		texturefile = ""gfx/interface/icons/text_icons/icon_text_military_size_1.dds"" } }");

            var sb2 = new StringBuilder();
            sb2.AppendLine(@"bitmapfonts = {");
            sb2.AppendLine(@"	bitmapfont = {");
            sb2.AppendLine(@"		name = ""GFX_text_military_size_1""");
            sb2.AppendLine(@"		texturefile = ""gfx/interface/icons/text_icons/icon_text_military_size_1.dds"" }");            
            sb2.AppendLine(@"}");


            var args = new ParserArgs()
            {
                ContentSHA = "sha",
                ModDependencies = new List<string> { "1" },
                File = "gfx\\gfx.gfx",
                Lines = sb.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                ModName = "fake"
            };
            var parser = new SimpleGFXParser(new CodeParser(), null);
            var result = parser.Parse(args).ToList();
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(1);
            for (int i = 0; i < 1; i++)
            {
                result[i].ContentSHA.Should().Be("sha");
                result[i].Dependencies.First().Should().Be("1");
                result[i].File.Should().Be("gfx\\gfx.gfx");
                switch (i)
                {

                    case 0:
                        result[i].Id.Should().Be("GFX_text_military_size_1");
                        result[i].Code.Should().Be(sb2.ToString().Trim().ReplaceTabs());
                        result[i].ValueType.Should().Be(Common.ValueType.Object);
                        break;
                    default:
                        break;
                }
                result[i].ModName.Should().Be("fake");
                result[i].Type.Should().Be("gfx\\gfx");
            }
        }

        /// <summary>
        /// Defines the test method Parse_bitmap_type_override_should_yield_results.
        /// </summary>
        [Fact]
        public void Parse_bitmap_type_override_should_yield_results()
        {
            DISetup.SetupContainer();

            var sb = new StringBuilder();
            sb.AppendLine(@"bitmapfonts = {");
            sb.AppendLine(@"	 bitmapfont_override = {");
            sb.AppendLine(@"		name = ""large_title_font""");
            sb.AppendLine(@"        ttf_font = ""Easter_bigger""");
            sb.AppendLine(@"        ttf_size = ""30""");
            sb.AppendLine(@"		languages = { ""l_russian"" ""l_polish"" }");
            sb.AppendLine(@"	 }	");
            sb.AppendLine(@"	 bitmapfont_override = {");
            sb.AppendLine(@"		name = ""large_title_font""");
            sb.AppendLine(@"        ttf_font = ""Easter_bigger""");
            sb.AppendLine(@"        ttf_size = ""30""");
            sb.AppendLine(@"		languages = { ""l_russian"" ""l_polish"" }");
            sb.AppendLine(@"	 }	");
            sb.AppendLine(@"}");


            var sb2 = new StringBuilder();
            sb2.AppendLine(@"bitmapfonts = {");
            sb2.AppendLine(@"	 bitmapfont_override = {");
            sb2.AppendLine(@"		name = ""large_title_font""");
            sb2.AppendLine(@"        ttf_font = ""Easter_bigger""");
            sb2.AppendLine(@"        ttf_size = ""30""");
            sb2.AppendLine(@"		languages = { ""l_russian"" ""l_polish"" }");
            sb2.AppendLine(@"	 }");
            sb2.AppendLine(@"}");


            var args = new ParserArgs()
            {
                ContentSHA = "sha",
                ModDependencies = new List<string> { "1" },
                File = "gfx\\gfx.gfx",
                Lines = sb.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                ModName = "fake"
            };
            var parser = new SimpleGFXParser(new CodeParser(), null);
            var result = parser.Parse(args).ToList();
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(2);
            for (int i = 0; i < 2; i++)
            {
                result[i].ContentSHA.Should().Be("sha");
                result[i].Dependencies.First().Should().Be("1");
                result[i].File.Should().Be("gfx\\gfx.gfx");
                switch (i)
                {

                    case 0:
                        result[i].Id.Should().Be("l_polish-l_russian-large_title_font");
                        result[i].Code.Should().Be(sb2.ToString().Trim().ReplaceTabs());
                        result[i].ValueType.Should().Be(Common.ValueType.Object);
                        break;
                    case 1:
                        result[i].Id.Should().Be("l_polish-l_russian-large_title_font");
                        result[i].Code.Should().Be(sb2.ToString().Trim().ReplaceTabs());
                        result[i].ValueType.Should().Be(Common.ValueType.Object);
                        break;
                    default:
                        break;
                }
                result[i].ModName.Should().Be("fake");
                result[i].Type.Should().Be("gfx\\gfx");
            }
        }

        /// <summary>
        /// Defines the test method Parse_variable_should_yield_results.
        /// </summary>
        [Fact]
        public void Parse_variable_should_yield_results()
        {
            DISetup.SetupContainer();

            var sb = new StringBuilder();
            sb.AppendLine(@"@test1 = 0");
            sb.AppendLine(@"spriteTypes = {");
            sb.AppendLine(@"	@test2 = 1");
            sb.AppendLine(@"	spriteType = {");
            sb.AppendLine(@"		name = ""GFX_dmm_mod_1""");
            sb.AppendLine(@"	}");
            sb.AppendLine(@"}");

            var args = new ParserArgs()
            {
                ContentSHA = "sha",
                ModDependencies = new List<string> { "1" },
                File = "gfx\\gfx.gfx",
                Lines = sb.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries),
                ModName = "fake"
            };
            var parser = new SimpleGFXParser(new CodeParser(), null);
            var result = parser.Parse(args).ToList();
            result.Should().NotBeNullOrEmpty();
            result.Count().Should().Be(3);
            for (int i = 0; i < 2; i++)
            {
                result[i].ContentSHA.Should().Be("sha");
                result[i].Dependencies.First().Should().Be("1");
                result[i].File.Should().Be("gfx\\gfx.gfx");
                switch (i)
                {

                    case 0:
                        result[i].Id.Should().Be("@test1");                        
                        result[i].ValueType.Should().Be(Common.ValueType.Variable);
                        break;
                    case 1:
                        result[i].Id.Should().Be("@test2");                        
                        result[i].ValueType.Should().Be(Common.ValueType.Variable);
                        break;
                    case 2:
                        result[i].Id.Should().Be("GFX_dmm_mod_1");
                        result[i].ValueType.Should().Be(Common.ValueType.Object);
                        break;
                    default:
                        break;
                }
                result[i].ModName.Should().Be("fake");
                result[i].Type.Should().Be("gfx\\gfx");
            }
        }
    }
}
