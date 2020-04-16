﻿// ***********************************************************************
// Assembly         : IronyModManager.Services.Tests
// Author           : Mario
// Created          : 02-24-2020
//
// Last Modified By : Mario
// Last Modified On : 04-16-2020
// ***********************************************************************
// <copyright file="ModServiceTests.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using IronyModManager.IO.Common;
using IronyModManager.IO.Common.Mods;
using IronyModManager.IO.Common.Readers;
using IronyModManager.IO.Mods.Models;
using IronyModManager.Models;
using IronyModManager.Models.Common;
using IronyModManager.Parser;
using IronyModManager.Parser.Common;
using IronyModManager.Parser.Common.Args;
using IronyModManager.Parser.Common.Definitions;
using IronyModManager.Parser.Common.Mod;
using IronyModManager.Parser.Definitions;
using IronyModManager.Parser.Mod;
using IronyModManager.Services.Common;
using IronyModManager.Shared;
using IronyModManager.Storage.Common;
using IronyModManager.Tests.Common;
using Moq;
using Xunit;
using FileInfo = IronyModManager.IO.FileInfo;

namespace IronyModManager.Services.Tests
{
    /// <summary>
    /// Class ModServiceTests.
    /// </summary>
    public class ModServiceTests
    {
        /// <summary>
        /// Setups the mock case.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="parserManager">The parser manager.</param>
        /// <param name="modParser">The mod parser.</param>
        private void SetupMockCase(Mock<IReader> reader, Mock<IParserManager> parserManager, Mock<IModParser> modParser)
        {
            var fileInfos = new List<IFileInfo>()
            {
                new FileInfo()
                {
                    Content = new List<string>() { "1" },
                    FileName = "fake1.txt",
                    IsBinary = false
                },
                new FileInfo()
                {
                    Content = new List<string>() { "2" } ,
                    FileName = "fake2.txt",
                    IsBinary = false
                }
            };
            reader.Setup(s => s.Read(It.IsAny<string>())).Returns(fileInfos);

            modParser.Setup(s => s.Parse(It.IsAny<IEnumerable<string>>())).Returns((IEnumerable<string> values) =>
            {
                return new ModObject()
                {
                    FileName = values.First(),
                    Name = values.First()
                };
            });

            parserManager.Setup(s => s.Parse(It.IsAny<ParserManagerArgs>())).Returns((ParserManagerArgs args) =>
            {
                return new List<IDefinition>() { new Definition()
                {
                    Code = args.File,
                    File = args.File,
                    ContentSHA = args.File,
                    Id = args.File,
                    Type = args.ModName
                } };
            });
        }
        /// <summary>
        /// Defines the test method Should_return_installed_mods.
        /// </summary>
        [Fact]
        public void Should_return_installed_mods()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var mapper = new Mock<IMapper>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName
                };
            });

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.GetInstalledMods(new Game() { UserDirectory = "fake1", WorkshopDirectory = "fake2" });
            result.Count().Should().Be(2);
            result.First().FileName.Should().Be("1");
            result.Last().FileName.Should().Be("2");
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="storageProvider">The storage provider.</param>
        /// <param name="modParser">The mod parser.</param>
        /// <param name="parserManager">The parser manager.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="modWriter">The mod writer.</param>
        /// <param name="gameService">The game service.</param>
        /// <param name="modPatchExporter">The mod patch exporter.</param>
        /// <returns>ModService.</returns>
        private ModService GetService(Mock<IStorageProvider> storageProvider, Mock<IModParser> modParser, Mock<IParserManager> parserManager, Mock<IReader> reader, Mock<IMapper> mapper, Mock<IModWriter> modWriter, Mock<IGameService> gameService, Mock<IModPatchExporter> modPatchExporter)
        {
            return new ModService(reader.Object, parserManager.Object, modParser.Object, modWriter.Object, modPatchExporter.Object, gameService.Object, storageProvider.Object, mapper.Object);
        }

        /// <summary>
        /// Defines the test method Should_throw_exception_when_no_game_specified_when_fetching_installed_mods.
        /// </summary>
        [Fact]
        public void Should_throw_exception_when_no_game_specified_when_fetching_installed_mods()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            try
            {
                service.GetInstalledMods(null);
            }
            catch (Exception ex)
            {
                ex.GetType().Should().Be(typeof(ArgumentNullException));
            }
        }

        /// <summary>
        /// Defines the test method Should_not_return_any_mod_objects_when_no_game_or_mods.
        /// </summary>
        [Fact]
        public void Should_not_return_any_mod_objects_when_no_game_or_mods()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.GetModObjects(null, new List<IMod>());
            result.Should().BeNull();

            result = service.GetModObjects(new Game(), new List<IMod>());
            result.Should().BeNull();

            result = service.GetModObjects(new Game(), null);
            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_return_mod_objects_when_using_fully_qualified_path.
        /// </summary>
        [Fact]
        public void Should_return_mod_objects_when_using_fully_qualified_path()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.GetModObjects(new Game(), new List<IMod>()
            {
                new Mod()
                {
                    FileName = Assembly.GetExecutingAssembly().Location,
                    Name = "fake"
                }
            });
            result.GetAll().Count().Should().Be(2);
            var ordered = result.GetAll().OrderBy(p => p.Id);
            ordered.First().Id.Should().Be("fake1.txt");
            ordered.Last().Id.Should().Be("fake2.txt");
        }

        /// <summary>
        /// Defines the test method Should_return_mod_objects_when_using_user_directory.
        /// </summary>
        [Fact]
        public void Should_return_mod_objects_when_using_user_directory()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.GetModObjects(new Game() { UserDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), WorkshopDirectory = "fake1" }, new List<IMod>()
            {
                new Mod()
                {
                    FileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location),
                    Name = "fake"
                }
            });
            result.GetAll().Count().Should().Be(2);
            var ordered = result.GetAll().OrderBy(p => p.Id);
            ordered.First().Id.Should().Be("fake1.txt");
            ordered.Last().Id.Should().Be("fake2.txt");
        }

        /// <summary>
        /// Defines the test method Should_return_mod_objects_when_using_workshop_directory.
        /// </summary>
        [Fact]
        public void Should_return_mod_objects_when_using_workshop_directory()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.GetModObjects(new Game() { WorkshopDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), UserDirectory = "fake1" }, new List<IMod>()
            {
                new Mod()
                {
                    FileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location),
                    Name = "fake"
                }
            });
            result.GetAll().Count().Should().Be(2);
            var ordered = result.GetAll().OrderBy(p => p.Id);
            ordered.First().Id.Should().Be("fake1.txt");
            ordered.Last().Id.Should().Be("fake2.txt");
        }

        /// <summary>
        /// Defines the test method Should_return_steam_url.
        /// </summary>
        [Fact]
        public void Should_return_steam_url()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var url = service.BuildModUrl(new Mod()
            {
                RemoteId = 1,
                Source = ModSource.Steam
            });
            url.Should().Be("https://steamcommunity.com/sharedfiles/filedetails/?id=1");
        }

        /// <summary>
        /// Defines the test method Should_return_paradox_url.
        /// </summary>
        [Fact]
        public void Should_return_paradox_url()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var url = service.BuildModUrl(new Mod()
            {
                RemoteId = 1,
                Source = ModSource.Paradox
            });
            url.Should().Be("https://mods.paradoxplaza.com/mods/1/Any");
        }

        /// <summary>
        /// Defines the test method Should_return_empty_url.
        /// </summary>
        [Fact]
        public void Should_return_empty_url()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var url = service.BuildModUrl(new Mod()
            {
                RemoteId = 1,
                Source = ModSource.Local
            });
            url.Should().BeNullOrEmpty();
        }

        /// <summary>
        /// Defines the test method Should_export_mods.
        /// </summary>
        [Fact]
        public async Task Should_export_mods()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName
                };
            });
            gameService.Setup(s => s.GetSelected()).Returns(new Game()
            {
                Type = "test",
                UserDirectory = "C:\\users\\fake"
            });
            modWriter.Setup(p => p.DescriptorExistsAsync(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return Task.FromResult(true);
            });
            modWriter.Setup(p => p.ApplyModsAsync(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return Task.FromResult(true);
            });

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.ExportModsAsync(new List<IMod> { new Mod() { DescriptorFile = "mod/fake.mod" } }, new List<IMod> { new Mod() { DescriptorFile = "mod/fake.mod" } }, "fake");
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_export_mods_when_no_selected_game.
        /// </summary>
        [Fact]
        public async Task Should_not_export_mods_when_no_selected_game()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName
                };
            });
            gameService.Setup(s => s.GetSelected()).Returns(() =>
            {
                return null;
            });
            modWriter.Setup(p => p.ApplyModsAsync(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return Task.FromResult(true);
            });
            modWriter.Setup(p => p.DescriptorExistsAsync(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return Task.FromResult(true);
            });

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.ExportModsAsync(new List<IMod> { new Mod() }, new List<IMod> { new Mod() }, "fake");
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_export_mods_when_collection_null_or_empty.
        /// </summary>
        [Fact]
        public async Task Should_not_export_mods_when_collection_null()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName
                };
            });
            gameService.Setup(s => s.GetSelected()).Returns(new Game()
            {
                Type = "test"
            });
            modWriter.Setup(p => p.ApplyModsAsync(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return Task.FromResult(true);
            });
            modWriter.Setup(p => p.DescriptorExistsAsync(It.IsAny<ModWriterParameters>())).Returns((ModWriterParameters p) =>
            {
                return Task.FromResult(true);
            });

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.ExportModsAsync(null, null, "fake");
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_find_filename_conflicts.
        /// </summary>
        [Fact]
        public void Should_find_filename_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(2);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(1);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAll().All(p => p.ModName == "test1" || p.ModName == "test2").Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_find_definition_conflicts.
        /// </summary>
        [Fact]
        public void Should_find_definition_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(2);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(2);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAll().All(p => p.ModName == "test1" || p.ModName == "test2").Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_find_override_conflicts.
        /// </summary>
        [Fact]
        public void Should_not_find_override_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test3",
                    ValueType = Parser.Common.ValueType.Object,
                    Dependencies = new List<string>() { "test1", "test2" }
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(0);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
        }

        /// <summary>
        /// Defines the test method Should_not_find_dependency_conflicts.
        /// </summary>
        [Fact]
        public void Should_not_find_dependency_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test3",
                    ValueType = Parser.Common.ValueType.Object,
                    Dependencies = new List<string>() { "test1" }
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(0);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
        }

        /// <summary>
        /// Defines the test method Should_find_dependency_conflicts.
        /// </summary>
        [Fact]
        public void Should_find_dependency_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test3",
                    ValueType = Parser.Common.ValueType.Object,
                    Dependencies = new List<string>() { "test2" }
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(2);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(1);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAll().All(p => p.ModName == "test1" || p.ModName == "test3").Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_find_multiple_dependency_conflicts.
        /// </summary>
        [Fact]
        public void Should_find_multiple_dependency_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test3",
                    ValueType = Parser.Common.ValueType.Object,
                    Dependencies = new List<string>() { "test1", "test2" }
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "f",
                    Type = "events",
                    Id = "a",
                    ModName = "test4",
                    ValueType = Parser.Common.ValueType.Object
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(2);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(1);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAll().All(p => p.ModName == "test3" || p.ModName == "test4").Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_find_variable_conflicts_in_different_filenames.
        /// </summary>
        [Fact]
        public void Should_not_include_variable_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a1",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a2",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Variable
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "a",
                    Type = "events",
                    Id = "a1",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "b",
                    Id = "a2",
                    Type= "events",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Variable
                },
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.Conflicts.GetAll().Count().Should().Be(0);
            result.Conflicts.GetAllFileKeys().Count().Should().Be(0);
            result.OrphanConflicts.GetAll().Count().Should().Be(0);
        }

        /// <summary>
        /// Defines the test method Should_return_all_conflicts.
        /// </summary>
        [Fact]
        public void Should_return_all_conflicts()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();

            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                }
            };
            var indexed = new IndexedDefinitions();
            indexed.InitMap(definitions);
            var result = service.FindConflicts(indexed);
            result.AllConflicts.GetAll().Count().Should().Be(2);
        }

        /// <summary>
        /// Defines the test method Should_not_apply_mod_patch_when_no_game_selected_or_parameters_null.
        /// </summary>
        [Fact]
        public async Task Should_not_apply_mod_patch_when_no_game_selected_or_parameters_null()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var c = new ConflictResult();
            var result = await service.ApplyModPatchAsync(c, new Definition(), "colname");
            result.Should().BeFalse();

            result = await service.ApplyModPatchAsync(null, new Definition(), "colname");
            result.Should().BeFalse();

            result = await service.ApplyModPatchAsync(c, null, "colname");
            result.Should().BeFalse();

            result = await service.ApplyModPatchAsync(c, new Definition(), null);
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_apply_mod_patch_when_nothing_to_merge.
        /// </summary>
        [Fact]
        public async Task Should_not_apply_mod_patch_when_nothing_to_merge()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName,
                    Name = o.Name
                };
            });
            SetupMockCase(reader, parserManager, modParser);

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var indexed = new IndexedDefinitions();
            indexed.InitMap(new List<IDefinition>());
            var c = new ConflictResult()
            {
                AllConflicts = indexed,
                Conflicts = indexed,
                OrphanConflicts = indexed,
                ResolvedConflicts = indexed
            };
            var result = await service.ApplyModPatchAsync(c, new Definition() { ModName = "test", ValueType = Parser.Common.ValueType.Object }, "colname");
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_return_true_when_applying_patches.
        /// </summary>
        [Fact]
        public async Task Should_return_true_when_applying_patches()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            SetupMockCase(reader, parserManager, modParser);
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName,
                    Name = o.Name
                };
            });
            modWriter.Setup(p => p.CreateModDirectoryAsync(It.IsAny<ModWriterParameters>())).Returns(Task.FromResult(true));
            modWriter.Setup(p => p.WriteDescriptorAsync(It.IsAny<ModWriterParameters>())).Returns(Task.FromResult(true));
            modPatchExporter.Setup(p => p.SaveStateAsync(It.IsAny<ModPatchExporterParameters>())).Returns(Task.FromResult(true));
            modWriter.Setup(p => p.PurgeModDirectoryAsync(It.IsAny<ModWriterParameters>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            modPatchExporter.Setup(p => p.ExportDefinitionAsync(It.IsAny<ModPatchExporterParameters>())).ReturnsAsync((ModPatchExporterParameters p) =>
            {
                if (p.Definitions.Count() > 0)
                {
                    return true;
                }
                return false;
            });

            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
            };
            var all = new IndexedDefinitions();
            all.InitMap(definitions);

            var orphan = new IndexedDefinitions();
            orphan.InitMap(new List<IDefinition>());

            var resolved = new IndexedDefinitions();
            resolved.InitMap(new List<IDefinition>());

            var c = new ConflictResult()
            {
                AllConflicts = all,
                Conflicts = all,
                OrphanConflicts = orphan,
                ResolvedConflicts = resolved
            };
            var result = await service.ApplyModPatchAsync(c, new Definition() { ModName = "1" }, "colname");
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_create_patch_definition.
        /// </summary>
        [Fact]
        public async Task Should_not_create_patch_definition()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IDefinition>(It.IsAny<IDefinition>())).Returns((IDefinition o) =>
            {
                return new Definition()
                {
                    File = o.File
                };
            });
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.CreatePatchDefinitionAsync(null, "fake");
            result.Should().BeNull();

            result = await service.CreatePatchDefinitionAsync(new Definition() { File = "1" }, null);
            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_not_create_patch_definition_when_no_selected_game.
        /// </summary>
        [Fact]
        public async Task Should_not_create_patch_definition_when_no_selected_game()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);
            mapper.Setup(s => s.Map<IDefinition>(It.IsAny<IDefinition>())).Returns((IDefinition o) =>
            {
                return new Definition()
                {
                    File = o.File
                };
            });
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var result = await service.CreatePatchDefinitionAsync(new Definition() { File = "1" }, "fake");
            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_create_patch_definition.
        /// </summary>
        [Fact]
        public async Task Should_create_patch_definition()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IDefinition>(It.IsAny<IDefinition>())).Returns((IDefinition o) =>
            {
                return new Definition()
                {
                    File = o.File
                };
            });
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.CreatePatchDefinitionAsync(new Definition() { File = "1" }, "fake");
            result.Should().NotBeNull();
            result.ModName.Should().Be("IronyModManager_fake");
        }

        /// <summary>
        /// Defines the test method Should_create_patch_definition_and_overwrite_code_from_history.
        /// </summary>
        [Fact]
        public async Task Should_create_patch_definition_and_overwrite_code_from_history()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IDefinition>(It.IsAny<IDefinition>())).Returns((IDefinition o) =>
            {
                return new Definition()
                {
                    File = o.File,
                    Id = o.Id,
                    Type = o.Type
                };
            });
            modPatchExporter.Setup(p => p.GetPatchStateAsync(It.IsAny<ModPatchExporterParameters>())).ReturnsAsync((ModPatchExporterParameters p) =>
            {
                var res = new PatchState()
                {
                    Conflicts = new List<IDefinition>(),
                    ResolvedConflicts = new List<IDefinition>(),
                    OrphanConflicts = new List<IDefinition>(),
                    ConflictHistory = new List<IDefinition>() { new Definition() { File = "1", Id = "test", Type = "events", Code = "ab" } }
                };
                return res;
            });
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.CreatePatchDefinitionAsync(new Definition() { File = "1", Id = "test", Type = "events", Code = "a" }, "fake");
            result.Should().NotBeNull();
            result.ModName.Should().Be("IronyModManager_fake");
            result.Code.Should().Be("ab");
        }

        /// <summary>
        /// Defines the test method Should_not_load_patch_state_when_no_selected_game.
        /// </summary>
        [Fact]
        public async Task Should_not_load_patch_state_when_no_selected_game()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var indexed = new IndexedDefinitions();
            indexed.InitMap(new List<IDefinition>());
            var c = new ConflictResult()
            {
                AllConflicts = indexed,
                Conflicts = indexed,
                OrphanConflicts = indexed,
                ResolvedConflicts = indexed
            };
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.LoadPatchStateAsync(c, "fake");
            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_not_load_patch_state.
        /// </summary>
        [Fact]
        public async Task Should_not_load_patch_state()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var indexed = new IndexedDefinitions();
            indexed.InitMap(new List<IDefinition>());
            var c = new ConflictResult()
            {
                AllConflicts = indexed,
                Conflicts = indexed,
                OrphanConflicts = indexed,
                ResolvedConflicts = indexed
            };
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.LoadPatchStateAsync(c, null);
            result.Should().BeNull();

            result = await service.LoadPatchStateAsync(null, "fake");
            result.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Should_load_patch_state.
        /// </summary>
        [Fact]
        public async Task Should_load_patch_state()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IConflictResult>(It.IsAny<IConflictResult>())).Returns((IConflictResult o) =>
            {
                return new ConflictResult()
                {
                    AllConflicts = o.Conflicts,
                    Conflicts = o.Conflicts,
                    OrphanConflicts = o.OrphanConflicts,
                    ResolvedConflicts = o.ResolvedConflicts
                };
            });
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
            };
            modPatchExporter.Setup(p => p.GetPatchStateAsync(It.IsAny<ModPatchExporterParameters>())).ReturnsAsync((ModPatchExporterParameters p) =>
            {
                var res = new PatchState()
                {
                    Conflicts = definitions,
                    ResolvedConflicts = definitions,
                    OrphanConflicts = new List<IDefinition>()
                };
                return res;
            });
            modWriter.Setup(p => p.PurgeModDirectoryAsync(It.IsAny<ModWriterParameters>(), It.IsAny<bool>())).Returns(Task.FromResult(true));

            var all = new IndexedDefinitions();
            all.InitMap(definitions);

            var orphan = new IndexedDefinitions();
            orphan.InitMap(new List<IDefinition>());

            var resolved = new IndexedDefinitions();
            resolved.InitMap(new List<IDefinition>());

            var c = new ConflictResult()
            {
                AllConflicts = all,
                Conflicts = all,
                OrphanConflicts = orphan,
            };
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.LoadPatchStateAsync(c, "fake");
            result.Conflicts.GetAll().Count().Should().Be(2);
            result.ResolvedConflicts.GetAll().Count().Should().Be(2);
        }

        /// <summary>
        /// Defines the test method Should_load_patch_state_and_remove_different.
        /// </summary>
        [Fact]
        public async Task Should_load_patch_state_and_remove_different()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake"
            });
            mapper.Setup(s => s.Map<IConflictResult>(It.IsAny<IConflictResult>())).Returns((IConflictResult o) =>
            {
                return new ConflictResult()
                {
                    AllConflicts = o.Conflicts,
                    Conflicts = o.Conflicts,
                    OrphanConflicts = o.OrphanConflicts,
                    ResolvedConflicts = o.ResolvedConflicts
                };
            });
            var definitions = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "a",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                },
            };
            var definitions2 = new List<IDefinition>()
            {
                new Definition()
                {
                    File = "events\\1.txt",
                    Code = "ab",
                    Id = "a",
                    Type= "events",
                    ModName = "test1",
                    ValueType = Parser.Common.ValueType.Object
                },
                new Definition()
                {
                    File = "events\\2.txt",
                    Code = "b",
                    Type = "events",
                    Id = "a",
                    ModName = "test2",
                    ValueType = Parser.Common.ValueType.Object
                }
            };
            modPatchExporter.Setup(p => p.GetPatchStateAsync(It.IsAny<ModPatchExporterParameters>())).ReturnsAsync((ModPatchExporterParameters p) =>
            {
                var res = new PatchState()
                {
                    Conflicts = definitions2,
                    ResolvedConflicts = definitions2,
                    OrphanConflicts = new List<IDefinition>()
                };
                return res;
            });
            modWriter.Setup(p => p.PurgeModDirectoryAsync(It.IsAny<ModWriterParameters>(), It.IsAny<bool>())).Returns(Task.FromResult(true));

            var all = new IndexedDefinitions();
            all.InitMap(definitions);

            var orphan = new IndexedDefinitions();
            orphan.InitMap(new List<IDefinition>());

            var resolved = new IndexedDefinitions();
            resolved.InitMap(new List<IDefinition>());

            var c = new ConflictResult()
            {
                AllConflicts = all,
                Conflicts = all,
                OrphanConflicts = orphan,
            };
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = await service.LoadPatchStateAsync(c, "fake");
            result.Conflicts.GetAll().Count().Should().Be(2);
            result.ResolvedConflicts.GetAll().Count().Should().Be(0);
        }


        /// <summary>
        /// Defines the test method Should_not_be_a_patch_mod.
        /// </summary>
        [Fact]
        public void Should_not_be_a_patch_mod()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.IsPatchMod(new Mod()
            {
                Name = "test"
            });
            result.Should().BeFalse();
            result = service.IsPatchMod(default(Mod));
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_be_a_patch_mod.
        /// </summary>
        [Fact]
        public void Should_be_a_patch_mod()
        {
            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);
            var result = service.IsPatchMod(new Mod()
            {
                Name = "IronyModManager_fake_collection"
            });
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_install_mods_when_no_game.
        /// </summary>
        [Fact]
        public async Task Should_not_install_mods_when_no_game()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var result = await service.InstallModsAsync();
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_install_mods.
        /// </summary>
        [Fact]
        public async Task Should_not_install_mods()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            SetupMockCase(reader, parserManager, modParser);

            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = "C:\\workshop"
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName
                };
            });

            var result = await service.InstallModsAsync();
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_install_mods.
        /// </summary>
        [Fact]
        public async Task Should_install_mods()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            SetupMockCase(reader, parserManager, modParser);

            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = AppDomain.CurrentDomain.BaseDirectory,
                WorkshopDirectory = "C:\\workshop"
            });
            mapper.Setup(s => s.Map<IMod>(It.IsAny<IModObject>())).Returns((IModObject o) =>
            {
                return new Mod()
                {
                    FileName = o.FileName
                };
            });
            modWriter.Setup(p => p.WriteDescriptorAsync(It.IsAny<ModWriterParameters>())).Returns(Task.FromResult(true));
            reader.Setup(p =>p.GetFileInfo(It.IsAny<string>(), It.IsAny<string>())).Returns((string root, string path) =>
            {
                var sb = new System.Text.StringBuilder(115);
                sb.AppendLine(@"path=""c:/fake""");
                sb.AppendLine(@"name=""Fake""");
                sb.AppendLine(@"picture=""thumbnail.png""");
                sb.AppendLine(@"tags={");
                sb.AppendLine(@"	""Gameplay""");
                sb.AppendLine(@"	""Fixes""");
                sb.AppendLine(@"}");
                sb.AppendLine(@"supported_version=""2.6.*""");

                return new FileInfo()
                {
                    Content = sb.ToString().SplitOnNewLine(),
                    ContentSHA = "test",
                    FileName = "fake.mod",
                    IsBinary = false
                };
            });

            var result = await service.InstallModsAsync();
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_lock_descriptors_when_no_game.
        /// </summary>
        [Fact]
        public async Task Should_not_lock_descriptors_when_no_game()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var result = await service.LockDescriptorsAsync(new List<IMod>(), true);
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_lock_descriptors_when_no_mods.
        /// </summary>
        [Fact]
        public async Task Should_not_lock_descriptors_when_no_mods()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = "C:\\workshop"
            });
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var result = await service.LockDescriptorsAsync(new List<IMod>(), true);
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_lock_descriptors.
        /// </summary>
        [Fact]
        public async Task Should_lock_descriptors()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = "C:\\workshop"
            });
            modWriter.Setup(p => p.SetDescriptorLockAsync(It.IsAny<ModWriterParameters>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var result = await service.LockDescriptorsAsync(new List<IMod>() { new Mod() }, true);
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_delete_descriptors_when_no_game.
        /// </summary>
        [Fact]
        public async Task Should_not_delete_descriptors_when_no_game()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var result = await service.DeleteDescriptorsAsync(new List<IMod>());
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_not_delete_descriptors_when_no_mods.
        /// </summary>
        [Fact]
        public async Task Should_not_delete_descriptors_when_no_mods()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = "C:\\workshop"
            });
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var result = await service.DeleteDescriptorsAsync(new List<IMod>());
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_delete_descriptors.
        /// </summary>
        [Fact]
        public async Task Should_delete_descriptors()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = "C:\\workshop"
            });
            modWriter.Setup(p => p.DeleteDescriptorAsync(It.IsAny<ModWriterParameters>())).Returns(Task.FromResult(true));
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            var result = await service.DeleteDescriptorsAsync(new List<IMod>() { new Mod() });
            result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method Should_not_clean_collection_patch.
        /// </summary>
        [Fact]
        public async Task Should_not_clean_collection_patch()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            gameService.Setup(p => p.GetSelected()).Returns((IGame)null);

            var result = await service.CleanPatchCollectionAsync("fake");
            result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Should_clean_collection_patch.
        /// </summary>
        [Fact]
        public async Task Should_clean_collection_patch()
        {
            DISetup.SetupContainer();

            var storageProvider = new Mock<IStorageProvider>();
            var modParser = new Mock<IModParser>();
            var parserManager = new Mock<IParserManager>();
            var reader = new Mock<IReader>();
            var modWriter = new Mock<IModWriter>();
            var gameService = new Mock<IGameService>();
            var mapper = new Mock<IMapper>();
            var modPatchExporter = new Mock<IModPatchExporter>();
            var service = GetService(storageProvider, modParser, parserManager, reader, mapper, modWriter, gameService, modPatchExporter);

            gameService.Setup(p => p.GetSelected()).Returns(new Game()
            {
                Type = "Fake",
                UserDirectory = "C:\\Users\\Fake",
                WorkshopDirectory = "C:\\workshop"
            });
            modWriter.Setup(p => p.DeleteDescriptorAsync(It.IsAny<ModWriterParameters>())).Returns(Task.FromResult(true));
            modWriter.Setup(p => p.PurgeModDirectoryAsync(It.IsAny<ModWriterParameters>(), It.IsAny<bool>())).Returns(Task.FromResult(true));

            var result = await service.CleanPatchCollectionAsync("fake");
            result.Should().BeTrue();
        }


        /// <summary>
        /// Defines the test method Stellaris_Performance_profiling.
        /// </summary>

#if FUNCTIONAL_TEST
        [Fact(Timeout = 3000000)]
#else
        [Fact(Skip = "This is for functional testing only")]
#endif
        public void Stellaris_Performance_profiling()
        {
            DISetup.SetupContainer();

            var registration = new Services.Registrations.GameRegistration();
            registration.OnPostStartup();
            var game = DISetup.Container.GetInstance<IGameService>().Get().First(s => s.Type == "Stellaris");
            var svc = DISetup.Container.GetInstance<IModService>();
            var mods = DISetup.Container.GetInstance<IModService>().GetInstalledMods(game);
            var defs = DISetup.Container.GetInstance<IModService>().GetModObjects(game, mods);
        }
    }
}
