using NUnit.Framework;
using Game.Emotes;

namespace Game.Tests.EditMode
{
    public class EmoteSystemSelectionTests
    {
        [Test]
        public void SelectFirstReadyEmote_ReturnsNull_ForNullOrEmptyJson()
        {
            Assert.IsNull(EmoteSystem.SelectFirstReadyEmote(null));
            Assert.IsNull(EmoteSystem.SelectFirstReadyEmote(""));
        }

        [Test]
        public void SelectFirstReadyEmote_ReturnsNull_ForEmptyArray()
        {
            Assert.IsNull(EmoteSystem.SelectFirstReadyEmote("[]"));
        }

        [Test]
        public void SelectFirstReadyEmote_ReturnsNull_ForMalformedJson()
        {
            Assert.IsNull(EmoteSystem.SelectFirstReadyEmote("not json"));
        }

        [Test]
        public void SelectFirstReadyEmote_ReturnsName_ForSingleReadyEntry()
        {
            var json = "[{\"name\":\"wave\",\"type\":\"custom\",\"hold\":false,\"createdAt\":1,\"ready\":true,\"fps\":30,\"frameCount\":12}]";

            Assert.AreEqual("wave", EmoteSystem.SelectFirstReadyEmote(json));
        }

        [Test]
        public void SelectFirstReadyEmote_SkipsNotReadyEntries()
        {
            var json = "[{\"name\":\"wave\",\"ready\":false},{\"name\":\"dance\",\"ready\":true}]";

            Assert.AreEqual("dance", EmoteSystem.SelectFirstReadyEmote(json));
        }

        [Test]
        public void SelectFirstReadyEmote_ReturnsNull_WhenNoEntriesAreReady()
        {
            var json = "[{\"name\":\"wave\",\"ready\":false},{\"name\":\"dance\",\"ready\":false}]";

            Assert.IsNull(EmoteSystem.SelectFirstReadyEmote(json));
        }

        [Test]
        public void SelectFirstReadyEmote_ReturnsFirstReadyEntry_WhenMultipleAreReady()
        {
            var json = "[{\"name\":\"wave\",\"ready\":true},{\"name\":\"dance\",\"ready\":true}]";

            Assert.AreEqual("wave", EmoteSystem.SelectFirstReadyEmote(json));
        }

        [Test]
        public void SelectFirstReadyEmote_SkipsReadyEntryWithNoName()
        {
            var json = "[{\"name\":\"\",\"ready\":true},{\"name\":\"dance\",\"ready\":true}]";

            Assert.AreEqual("dance", EmoteSystem.SelectFirstReadyEmote(json));
        }
    }
}
