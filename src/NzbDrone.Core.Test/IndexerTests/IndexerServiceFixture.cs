using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class IndexerServiceFixture : DbTest<IndexerFactory, IndexerDefinition>
    {
        private List<IIndexer> _indexers;

        [SetUp]
        public void Setup()
        {
            _indexers = new List<IIndexer>();

            Mocker.SetConstant<IEnumerable<IIndexer>>(_indexers);
        }

        [Test]
        public void should_remove_missing_indexers_on_startup()
        {
            var repo = Mocker.Resolve<IndexerRepository>();

            Mocker.SetConstant<IIndexerRepository>(repo);

            var existingIndexers = Builder<IndexerDefinition>.CreateNew().BuildNew();

            repo.Insert(existingIndexers);

            Subject.Handle(new ApplicationStartedEvent());

            AllStoredModels.Should().NotContain(c => c.Id == existingIndexers.Id);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
