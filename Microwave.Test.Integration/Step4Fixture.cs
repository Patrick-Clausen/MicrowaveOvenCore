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
    public class Step4Fixture
    {
        //SUBSTITUTES
        private IPowerTube powerTubeSubstitute;
        private IDisplay displaySubstitute;
        private ILight lightSubstitute;

        //SUT
        private CookController uut;
        private IUserInterface ui;
        private ITimer timer;
        private IDoor door;
        private IButton powerButton;
        private IButton timeButton;
        private IButton startCancelButton;

        [SetUp]
        public void Setup()
        {
            powerTubeSubstitute = Substitute.For<IPowerTube>();
            displaySubstitute = Substitute.For<IDisplay>();
            lightSubstitute = Substitute.For<ILight>();

            powerButton = new Button();
            timeButton = new Button();
            startCancelButton = new Button();
            door = new Door();
            timer = new Timer();

            uut = new CookController(timer, displaySubstitute, powerTubeSubstitute);

            ui = new UserInterface(
                powerButton,
                timeButton,
                startCancelButton,
                door,
                displaySubstitute,
                lightSubstitute,
                uut);

            uut.UI = ui;
        }

        /*
         * The buttons have pretty much no code besides from invoking the event
         * Meaning - These tests might seem a bit silly
         * But we gotta make sure that the system responds correctly to each button-press at each time!
         * We're integrating - It's not about the button, it's about how the system reacts.
         */

        #region PowerButton

        //PowerButtonPressedOnce_IsInReadyState_ShowsPowerLevelDisplayWith50
        [Test]
        public void PowerButtonPressedOnce_IsInReadyState_ShowsPowerLevelDisplay()
        {
            //ARRANGE
            //No arrange needed, already in correct state

            //ACT
            powerButton.Press();

            //ASSERT
            displaySubstitute.Received().ShowPower(50);
        }

        //PowerButtonPressedOnce_IsInReadyState_ShowsPowerLevelDisplayWith100
        [Test]
        public void PowerButtonPressedTwice_IsInReadyState_ShowsPowerLevelDisplay()
        {
            //ARRANGE
            //No arrange needed, already in correct state

            //ACT
            powerButton.Press();
            powerButton.Press();

            //ASSERT
            displaySubstitute.Received().ShowPower(100);
        }

        //PowerButtonPressedOnce_IsInReadyState_ShowsPowerLevelDisplayWith150
        [Test]
        public void PowerButtonPressedThrice_IsInReadyState_ShowsPowerLevelDisplay()
        {
            //ARRANGE
            //No arrange needed, already in correct state

            //ACT
            powerButton.Press();
            powerButton.Press();
            powerButton.Press();

            //ASSERT
            displaySubstitute.Received().ShowPower(150);
        }
        [Test]
        public void PowerButtonPressedAndMicrowaveStarted_IsInReadyState_TurnsOnAt50()
        {
            //ARRANGE
            //No arrange needed, already in correct state

            //ACT
            powerButton.Press();
            timeButton.Press();
            startCancelButton.Press();

            //ASSERT
            powerTubeSubstitute.Received().TurnOn(50);
        }
        [Test]
        public void PowerButtonPressedTwiceAndMicrowaveStarted_IsInReadyState_TurnsOnAt100()
        {
            //ARRANGE
            //No arrange needed, already in correct state

            //ACT
            powerButton.Press();
            powerButton.Press();
            timeButton.Press();
            startCancelButton.Press();

            //ASSERT
            powerTubeSubstitute.Received().TurnOn(100);
        }

        [Test]
        public void PowerButtonPressed_IsInTimeState_DoesNothing()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();

            //ACT
            powerButton.Press();

            //ASSERT
            displaySubstitute.DidNotReceive().ShowPower(50);
        }
        #endregion

        #region TimeButton
        [Test]
        public void TimeButtonPressedOnce_IsInSetPowerState_ShowsTimeDisplay()
        {
            //ARRANGE
            powerButton.Press();

            //ACT
            timeButton.Press();

            //ASSERT
            displaySubstitute.Received().ShowTime(1,0);
        }

        [Test]
        public void TimeButtonPressedTwice_IsInSetPowerState_ShowsTimeDisplay()
        {
            //ARRANGE
            powerButton.Press();

            //ACT
            timeButton.Press();
            timeButton.Press();

            //ASSERT
            displaySubstitute.Received().ShowTime(2,0);
        }

        [Test]
        public void TimeButtonPressedAndMicrowaveStarted_IsInPowerState_TurnsOnForAMinute()
        {
            //ARRANGE
            powerButton.Press();

            //Boilerplate to force a wait until ShowTime is called
            AutoResetEvent reset = new AutoResetEvent(false);

            powerTubeSubstitute
                .When(d => d.TurnOff())
                .Do(c =>
                {
                    reset.Set();
                });


            //ACT
            timeButton.Press();
            startCancelButton.Press();

            //ASSERT
            //We want to fail if power isn't turned off after a minute!
            Assert.That(reset.WaitOne(61000), Is.True);
        }

        [Test]
        public void TimeButtonPressedTwiceAndMicrowaveStarted_IsInPowerState_TurnsOnForTwoMinutes()
        {
            //ARRANGE
            powerButton.Press();

            //Boilerplate to force a wait until powertube is off
            AutoResetEvent reset = new AutoResetEvent(false);

            powerTubeSubstitute
                .When(d => d.TurnOff())
                .Do(c =>
                {
                    reset.Set();
                });


            //ACT
            timeButton.Press();
            timeButton.Press();
            startCancelButton.Press();

            //ASSERT
            //We want to fail if power isn't turned off after two minutes!
            Assert.That(reset.WaitOne(121000), Is.True);
        }

        #endregion

        #region StartCancelButton
        //Simplest version is already tested by PowerButton section -> Power, Time, Start-Cancel, so no need to repeat.
        /*
         * First of all, test "cancel" - That is, when power is pressed, then start-cancel, a reset happens
         */
        [Test]
        public void StartCancelButtonPressed_IsInPowerState_DisplayIsCleared()
        {
            //ARRANGE
            powerButton.Press();

            //ACT
            startCancelButton.Press();

            //ASSERT
            displaySubstitute.Received().Clear();
        }

        [Test]
        public void StartCancelButtonPressed_IsCooking_StopsCooking()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();

            //Using ShowTime to enforce runtime - is called once a second
            //Thereby, if called 4 times -> 5 seconds(+1 for initial time)
            //If start-cancel is pressed then -> Only runs 5 seconds!
            int eventCount = 0;


            displaySubstitute
                .When(d => d.ShowTime(Arg.Any<int>(), Arg.Any<int>()))
                .Do(c =>
                {
                    if (eventCount < 4)
                    {
                        eventCount++;
                    }
                    else
                    {
                        startCancelButton.Press();
                    }
                });


            //Boilerplate to force a wait until powertube is off
            AutoResetEvent reset = new AutoResetEvent(false);

            powerTubeSubstitute
                .When(d => d.TurnOff())
                .Do(c =>
                {
                    reset.Set();
                });

            //ACT
            startCancelButton.Press();

            //ASSERT
            //We want to fail if power isn't turned off after 6 seconds!
            Assert.That(reset.WaitOne(6000), Is.True);
        }
        #endregion



    }
}