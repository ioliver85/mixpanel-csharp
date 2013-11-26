﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;
using NUnit.Framework;

namespace Mixpanel.Tests
{
    [TestFixture]
    public class PropertiesDiggerTests
    {
        private PropertiesDigger _digger;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _digger = new PropertiesDigger();
            _now = DateTime.Now;
        }

        [Test]
        public void Get_StringKeyObjectValueDictionary_Parsed()
        {
            var inDic = new Dictionary<string, object>
            {
                {"property1", 1},
                {"property2", "val"},
                {"property3", _now}
            };

            var outDic = _digger.Get(inDic);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["property1"], Is.EqualTo(1));
            Assert.That(outDic["property2"], Is.EqualTo("val"));
            Assert.That(outDic["property3"], Is.EqualTo(_now));
        }

        [Test]
        public void Get_StringKeyNonObjectValueDictionary_Parsed()
        {
            var inDic = new Dictionary<string, decimal>
            {
                {"property1", 1M},
                {"property2", 2M}
            };

            var outDic = _digger.Get(inDic);
            Assert.That(outDic.Count, Is.EqualTo(2));
            Assert.That(outDic["property1"], Is.EqualTo(1M));
            Assert.That(outDic["property2"], Is.EqualTo(2M));
        }

        [Test]
        public void Get_NonStringKeyDictionary_Parsed()
        {
            var inDic = new Dictionary<object, object>
            {
                {"property1", 1M},
                {2, "val2"},
                {"property3", _now}
            };

            var outDic = _digger.Get(inDic);
            Assert.That(outDic.Count, Is.EqualTo(2));
            Assert.That(outDic["property1"], Is.EqualTo(1M));
            Assert.That(outDic["property3"], Is.EqualTo(_now));
        }


        [Test]
        public void Get_Hashtable_Parsed()
        {
            var hashtable = new Hashtable
            {
                {"property1", 1M},
                {2, "val2"},
                {"property3", _now}
            };

            var outDic = _digger.Get(hashtable);
            Assert.That(outDic.Count, Is.EqualTo(2));
            Assert.That(outDic["property1"], Is.EqualTo(1M));
            Assert.That(outDic["property3"], Is.EqualTo(_now));
        }

        [Test]
        public void Get_ExpandoObject_Parsed()
        {
            dynamic expando = new ExpandoObject();
            expando.property1 = 1M;
            expando.Property2 = "val";
            expando.property3 = _now;

            var outDic = _digger.Get(expando);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["property1"], Is.EqualTo(1M));
            Assert.That(outDic["Property2"], Is.EqualTo("val"));
            Assert.That(outDic["property3"], Is.EqualTo(_now));
        }

        [Test]
        public void Get_Dynamic_Parsed()
        {
            dynamic dyn = new
            {
                Property1 = 1M,
                Property2 = "val",
                Property3 = _now
            };

            var outDic = _digger.Get(dyn);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["Property1"], Is.EqualTo(1M));
            Assert.That(outDic["Property2"], Is.EqualTo("val"));
            Assert.That(outDic["Property3"], Is.EqualTo(_now));
        }

        internal class Test1
        {
            public decimal Property1 { get; set; }
            public string Property2 { get; set; }
            public DateTime Property3 { get; set; }
        }

        [Test]
        public void Get_Class_Parsed()
        {
            var test = new Test1
            {
                Property1 = 1M,
                Property2 = "val",
                Property3 = _now
            };

            var outDic = _digger.Get(test);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["Property1"], Is.EqualTo(1M));
            Assert.That(outDic["Property2"], Is.EqualTo("val"));
            Assert.That(outDic["Property3"], Is.EqualTo(_now));
        }


        internal class Test2
        {
            [MixpanelProperty("property_1")]
            public decimal Property1 { get; set; }

            public string Property2 { get; set; }

            [MixpanelProperty("property_3")]
            public DateTime Property3 { get; set; }
        }

        [Test]
        public void Get_ClassWithMixpanelPropertyAttr_Parsed()
        {
            var test = new Test2
            {
                Property1 = 1M,
                Property2 = "val",
                Property3 = _now
            };

            var outDic = _digger.Get(test);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["property_1"], Is.EqualTo(1M));
            Assert.That(outDic["Property2"], Is.EqualTo("val"));
            Assert.That(outDic["property_3"], Is.EqualTo(_now));
        }

        internal class Test3
        {
            [MixpanelProperty("property_1")]
            public decimal Property1 { get; set; }

            [IgnoreDataMember]
            public string Property2 { get; set; }

            [DataMember(Name = "property_3")]
            public DateTime Property3 { get; set; }

            public string Property4 { get; set; }
        }

        [Test]
        public void Get_ClassWithIgnoreDataMemberAttr_Parsed()
        {
            var test = new Test3
            {
                Property1 = 1M,
                Property2 = "val",
                Property3 = _now,
                Property4 = "p4"
            };

            var outDic = _digger.Get(test);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["property_1"], Is.EqualTo(1M));
            Assert.That(outDic["Property3"], Is.EqualTo(_now));
            Assert.That(outDic["Property4"], Is.EqualTo("p4"));
        }

        [DataContract]
        internal class Test4
        {
            [MixpanelProperty("mp_property1")]
            [DataMember(Name = "property1")]
            public decimal Property1 { get; set; }

            [IgnoreDataMember]
            public string Property2 { get; set; }

            [DataMember(Name = "property3")]
            public DateTime Property3 { get; set; }

            public string Property4 { get; set; }

            [MixpanelProperty("mp_property5")]
            public string Property5 { get; set; }

            [DataMember]
            public string Property6 { get; set; }
        }

        [Test]
        public void Get_ClassWithAllAttributes_Parsed()
        {
            var test = new Test4
            {
                Property1 = 1M,
                Property2 = "val",
                Property3 = _now,
                Property4 = "p4",
                Property5 = "p5",
                Property6 = "p6"
            };

            var outDic = _digger.Get(test);
            Assert.That(outDic.Count, Is.EqualTo(3));
            Assert.That(outDic["mp_property1"], Is.EqualTo(1M));
            Assert.That(outDic["property3"], Is.EqualTo(_now));
            Assert.That(outDic["Property6"], Is.EqualTo("p6"));
        }
    }
}
