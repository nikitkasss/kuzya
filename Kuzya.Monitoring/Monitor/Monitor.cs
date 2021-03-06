﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuzya.Data;
using Kuzya.Data.Repositories;
using Kuzya.Monitoring.Models;
using Kuzya.Monitoring.Settings;

namespace Kuzya.Monitoring.Monitor
{
    public class Monitor
    {
        private readonly MonitoringSettings _settings;
        private readonly string _connectionString;
        private readonly List<Site> _sites;

        public Monitor(MonitoringSettings settings, string connectionString)
        {
            _settings = settings;
            _connectionString = connectionString;

            // TODO: Check count of sites > 0?

            _sites = _settings.Sites.Select(InitializeSite).ToList();
            _sites.ForEach(x => x.NewFlats += OnNewFlats);
        }

        public event Action<List<Flat>> NewFlats;

        public void Run()
        {
            var tasks = _sites.Select(s => new Task(s.Run));
            Parallel.ForEach(tasks, t => t.Start());
        }

        private Site InitializeSite(SiteSettings settings)
        {
            var context = new KuzyaContext(_connectionString);
            var repository = new FlatRepository(context);

            return new Site(settings, repository);
        }

        private void OnNewFlats(List<Flat> flats)
        {
            NewFlats?.Invoke(flats);
        }
    }
}