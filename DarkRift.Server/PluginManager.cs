﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace DarkRift.Server
{
    /// <summary>
    ///     The manager of all plugins on the server.
    /// </summary>
    internal sealed class PluginManager : ExtendedPluginManagerBase<Plugin>, IPluginManager
    {
        /// <summary>
        ///     The server that owns this plugin manager.
        /// </summary>
        private readonly DarkRiftServer server;

        /// <summary>
        ///     The server's data manager.
        /// </summary>
        private readonly DataManager dataManager;

        /// <summary>
        ///     The server's log manager.
        /// </summary>
        private readonly LogManager logManager;

        /// <summary>
        ///     The server's log manager.
        /// </summary>
        private readonly MetricsManager metricsManager;

        /// <summary>
        ///     The server's plugin factory.
        /// </summary>
        private readonly PluginFactory pluginFactory;

        /// <summary>
        ///     Creates a new PluginManager.
        /// </summary>
        /// <param name="server">The server that owns this plugin manager.</param>
        /// <param name="dataManager">The server's data manager.</param>
        /// <param name="logManager">The server's log manager.</param>
        /// <param name="pluginFactory">The server's plugin factory.</param>
        /// <param name="logger">The logger for this manager.</param>
        /// <param name="metricsManager">The server's metrics manager.</param>
        internal PluginManager(DarkRiftServer server, DataManager dataManager, LogManager logManager, MetricsManager metricsManager, PluginFactory pluginFactory, Logger logger)
            : base(server, dataManager, pluginFactory, logger)
        {
            this.server = server;
            this.dataManager = dataManager;
            this.logManager = logManager;
            this.metricsManager = metricsManager;
            this.pluginFactory = pluginFactory;
        }

        /// <summary>
        ///     Loads the plugins found by the plugin factory.
        /// </summary>
        /// <param name="settings">The settings to load plugins with.</param>
        internal void LoadPlugins(ServerSpawnData.PluginsSettings settings)
        {
            var types = pluginFactory.GetAllSubtypes(typeof(Plugin));

            foreach (var type in types)
            {
                var s = settings.Plugins.FirstOrDefault(p => p.Type == type.Name);

                var loadData = new PluginLoadData(
                    type.Name,
                    server,
                    s?.Settings ?? new NameValueCollection(),
                    logManager.GetLoggerFor(type.Name),
                    metricsManager.GetMetricsCollectorFor(type.Name),
                    dataManager.GetResourceDirectory(type.Name)
                );

                if (s?.Load ?? settings.LoadByDefault)
                {
                    LoadPlugin(type.Name, type, loadData, null, true);
                }
            }
        }

        /// <inheritdoc/>
        public Plugin this[string name] => GetPlugin(name);

        /// <inheritdoc/>
        public Plugin GetPluginByName(string name)
        {
            return this[name];
        }

        /// <inheritdoc/>
        public T GetPluginByType<T>() where T : Plugin
        {
            return (T)GetPlugins().First((x) => x is T);
        }

        /// <inheritdoc/>
        public Plugin[] GetAllPlugins()
        {
            return GetPlugins().Where((p) => !p.Hidden).ToArray();
        }

        /// <inheritdoc/>
        public Plugin[] ActuallyGetAllPlugins()
        {
            return GetPlugins().ToArray();
        }
    }
}
