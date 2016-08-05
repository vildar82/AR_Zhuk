﻿using NUnit.Framework;
using AR_AreaZhuk.Insolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AR_Zhuk_Schema.Insolation;
using static AR_Zhuk_Schema.Insolation.LightingStringParser;

namespace AR_AreaZhuk.Insolation.Tests
{
    [TestFixture()]
    public class LightingStringParserTests
    {        
        [SetUp]
        public void Setup()
        {            
        }

        private void Expect (Lighting actual, Lighting expected)
        {
            CollectionAssert.AreEqual(actual.Indexes, expected.Indexes);
            CollectionAssert.AreEqual(actual.SideIndexes, expected.SideIndexes);
            Assert.AreEqual(actual.Side, expected.Side);
        }

        [Test]
        public void GetLightingsSimple1Test ()
        {
            string lightingstringFlat = "1";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { 1 },
                SideIndexes = new List<int>(),
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);
            
            Expect(actual, expected);
        }

        [Test]
        public void GetLightingsSimple2Test ()
        {
            string lightingstringFlat = "1-3";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { 1, 2, 3 },
                SideIndexes = new List<int>(),
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);
        }

        [Test]
        public void GetLightingsHard1Test ()
        {
            string lightingstringFlat = "1|2-3";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { -1, -2, 3 },
                SideIndexes = new List<int>(),
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);
        }

        [Test]
        public void GetLightingsHard2Test ()
        {
            string lightingstringFlat = "1-2|3";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { 1, -2, -3 },
                SideIndexes = new List<int>(),
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);
        }

        [Test]
        public void GetLightingsHard3Test ()
        {
            string lightingstringFlat = "1-2|3-4";
            var expected = new Lighting() {
                Indexes = new List<int> { 1, -2, -3, 4 },
                SideIndexes = new List<int>(),
                IsTopSide = true
            };

            var actual = GetLightings(lightingstringFlat, true);

            Expect(actual, expected);
        }

        [Test]
        public void GetLightingsSide1Test ()
        {
            string lightingstringFlat = "B,1|2";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { -1, -2 },
                SideIndexes = new List<int> { 1 },
                Side = Side.Right,
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);
        }        

        [Test]
        public void GetLightingsSide2Test ()
        {
            string lightingstringFlat = "2|3,B";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { -2, -3 },
                SideIndexes = new List<int> { 1 },
                Side = Side.Left,
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);
        }

        [Test]
        public void GetLightingsSide3Test ()
        {
            string lightingstringFlat = "B,1|2";
            bool isTop = false;
            var expected = new Lighting() {
                Indexes = new List<int> { -1, -2 },
                SideIndexes = new List<int> { 1 },
                Side = Side.Left,
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);            
        }

        [Test]
        public void GetLightingsSide4Test ()
        {
            string lightingstringFlat = "B|1";
            bool isTop = true;
            var expected = new Lighting() {
                Indexes = new List<int> { -1 },
                SideIndexes = new List<int> { -1 },
                Side = Side.Right,
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);            
        }

        [Test]
        public void GetLightingsSide5Test ()
        {
            string lightingstringFlat = "1|B";
            bool isTop = false;
            var expected = new Lighting() {
                Indexes = new List<int> { -1 },
                SideIndexes = new List<int> { -1 },
                Side = Side.Right,
                IsTopSide = false
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);            
        }

        [Test]
        public void GetLightingsSideHard1Test ()
        {
            string lightingstringFlat = "1-2|B1,B2";
            bool isTop = false;
            var expected = new Lighting() {
                Indexes = new List<int> { 1, -2 },
                SideIndexes = new List<int> { -1, 2 },
                Side = Side.Right,
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);           
        }

        [Test]
        public void GetLightingsSideHard2Test ()
        {
            string lightingstringFlat = "1|2,3|B";
            bool isTop = false;
            var expected = new Lighting() {
                Indexes = new List<int> { -1, -2, -3 },
                SideIndexes = new List<int> { -1 },
                Side = Side.Right,
                IsTopSide = isTop
            };

            var actual = GetLightings(lightingstringFlat, isTop);

            Expect(actual, expected);            
        }
    }
}