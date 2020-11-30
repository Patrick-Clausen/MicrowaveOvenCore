using System;
using System.IO;
using System.Threading;
using Microwave.Classes.Boundary;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Timer = Microwave.Classes.Boundary.Timer;

namespace Microwave.Test.Integration
{
    [TestFixture]
    public class Step8Fixture
    {
        //SUT
        private IOutput output;
        private CookController uut;
        private ILight light;
        private IUserInterface ui;
        private ITimer timer;
        private IDoor door;
        private IButton powerButton;
        private IButton timeButton;
        private IButton startCancelButton;
        private IPowerTube powerTube;
        private IDisplay display;

        [SetUp]
        public void Setup()
        {
            output = new Output();

            powerTube = new PowerTube(output);
            display = new Display(output);
            light = new Light(output);

            powerButton = new Button();
            timeButton = new Button();
            startCancelButton = new Button();

            door = new Door();
            timer = new Timer();

            uut = new CookController(timer, display, powerTube);

            ui = new UserInterface(
                powerButton,
                timeButton,
                startCancelButton,
                door,
                display,
                light,
                uut);

            uut.UI = ui;

            //Set standard streamwriter
            StreamWriter standardOut = new StreamWriter(Console.OpenStandardOutput());
            standardOut.AutoFlush = true;
            Console.SetOut(standardOut);
        }

        //OBS: These tests are somewhat redundant, as our output class has no extra functions and is merely a light wrapper for stdout. We are essentially
        //testing if Console.WriteLine is working as intended (which it hopefully should)

        [Test]
        public void DoorIsOpened_IsInReadyState_OutputLogfileReceivesCallLightIsTurnedOn()
        {
            //ARRANGE
            string expected = "Light is turned on";

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                door.Open();

                //ASSERT
                Assert.That(sw.ToString().TrimEnd('\r', '\n'), Is.EqualTo(expected));
            }
        }

        [Test]
        public void DoorIsClosed_IsInDoorIsOpenState_OutputLogfileReceivesCallLightIsTurnedOff()
        {
            //ARRANGE
            string expected = "Light is turned off";
            door.Open();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                door.Close();

                //ASSERT
                Assert.That(sw.ToString().TrimEnd('\r', '\n'), Is.EqualTo(expected));
            }
        }

        [Test]
        public void PowerButtonIsPressed_IsInReadyState_OutputLogfileReceivesCallDisplayShowsPower()
        {
            //ARRANGE
            string expected = "Display shows: 50 W";
            
            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                powerButton.Press();

                //ASSERT
                Assert.That(sw.ToString().TrimEnd('\r', '\n'), Is.EqualTo(expected));
            }
        }

        [Test]
        public void TimeButtonIsPressed_IsInSetPowerState_OutputLogfileReceivesCallDisplayShowsTime()
        {
            //ARRANGE
            string expected = "Display shows: 01:00";
            //For setting the state to SetPower
            powerButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                timeButton.Press();

                //ASSERT
                Assert.That(sw.ToString().TrimEnd('\r', '\n'), Is.EqualTo(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInSetTimeState_OutputLogfileReceivesCallLightIsTurnedOn()
        {
            //ARRANGE
            string expected = "Light is turned on";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInSetTimeState_OutputLogfileReceivesCallPowerTube()
        {
            //ARRANGE
            string expected = "PowerTube works with 50";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingState_OutputLogfileReceivesDisplayShowsTime()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "Display shows: 00:59";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(61000);

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingState_OutputLogfileReceivesPowerTubeTurnedOff()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "PowerTube turned off";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(61000);

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingState_OutputLogfileReceivesDisplayCleared()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "Display cleared";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(61000);

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingState_OutputLogfileReceivesLightTurnedOff()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "Light is turned off";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(61000);

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingStateAndDoorIsOpened_OutputLogfileReceivesPowerTubeTurnedOff()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "PowerTube turned off";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(1000);
                door.Open();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingStateAndDoorIsOpened_OutputLogfileReceivesDisplayCleared()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "Display cleared";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(1000);
                door.Open();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingStateAndStartCancelButtonIsPressed_OutputLogfileReceivesLightTurnedOff()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "Light is turned off";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(1000);
                startCancelButton.Press();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingStateAndStartCancelButtonIsPressed_OutputLogfileReceivesPowerTubeTurnedOff()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "PowerTube turned off";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(1000);
                startCancelButton.Press();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingStateAndStartCancelButtonIsPressed_OutputLogfileReceivesDisplayCleared()
        {
            //ARRANGE
            AutoResetEvent reset = new AutoResetEvent(false);
            string expected = "Display cleared";
            //For setting the state to SetPower
            powerButton.Press();
            timeButton.Press();

            using (StringWriter sw = new StringWriter())
            {
                Console.SetOut(sw);

                //ACT
                startCancelButton.Press();
                reset.WaitOne(1000);
                startCancelButton.Press();

                //ASSERT
                Assert.That(sw.ToString().Contains(expected));
            }
        }
    }
}