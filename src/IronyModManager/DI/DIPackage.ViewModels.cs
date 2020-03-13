﻿// ***********************************************************************
// Assembly         : IronyModManager
// Author           : Mario
// Created          : 01-11-2020
//
// Last Modified By : Mario
// Last Modified On : 03-09-2020
// ***********************************************************************
// <copyright file="DIPackage.ViewModels.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System;
using IronyModManager.Localization;
using IronyModManager.ViewModels;
using IronyModManager.ViewModels.Controls;
using Container = SimpleInjector.Container;

namespace IronyModManager.DI
{
    /// <summary>
    /// Class DIPackage.
    /// Implements the <see cref="SimpleInjector.Packaging.IPackage" />
    /// </summary>
    /// <seealso cref="SimpleInjector.Packaging.IPackage" />
    public partial class DIPackage
    {
        #region Methods

        /// <summary>
        /// Registers the view models.
        /// </summary>
        /// <param name="container">The container.</param>
        private void RegisterViewModels(Container container)
        {
            container.RegisterLocalization<MainWindowViewModel>();
            container.RegisterLocalization<ThemeControlViewModel>();
            container.RegisterLocalization<LanguageControlViewModel>();
            container.RegisterLocalization<MainControlViewModel>();
            container.RegisterLocalization<GameControlViewModel>();
            container.RegisterLocalization<InstalledModsControlViewModel>();
            container.RegisterLocalization<SortOrderControlViewModel>();
            container.RegisterLocalization<ModHolderControlViewModel>();
            container.RegisterLocalization<SearchModsControlViewModel>();
            container.RegisterLocalization<CollectionModsControlViewModel>();
            container.RegisterLocalization<AddNewCollectionControlViewModel>();
            container.RegisterLocalization<ExportModCollectionControlViewModel>();
        }

        #endregion Methods
    }
}
