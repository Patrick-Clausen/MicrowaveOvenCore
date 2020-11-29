using System.Threading;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Timer = Microwave.Classes.Boundary.Timer;

namespace Microwave.Test.Integration
{
    [TestFixture]
    public class Step2Fixture
    {
        //SUBSTITUTES
        private IDoor doorSubstitute;
        private IButton powerButtonSubstitute;
        private IButton timeButtonSubstitute;
        private IButton startCancelButtonSubstitute;
        private IPowerTube powerTubeSubstitute;
        private IDisplay displaySubstitute;
        private ILight lightSubstitute;

        //SUT
        private CookController uut;
        private IUserInterface ui;
        private ITimer timer;

        [SetUp]
        public void Setup()
        {
            doorSubstitute = Substitute.For<IDoor>();
            powerButtonSubstitute = Substitute.For<IButton>();
            timeButtonSubstitute = Substitute.For<IButton>();
            startCancelButtonSubstitute = Substitute.For<IButton>();
            powerTubeSubstitute = Substitute.For<IPowerTube>();
            displaySubstitute = Substitute.For<IDisplay>();
            lightSubstitute = Substitute.For<ILight>();

            timer = new Timer();

            uut = new CookController(timer, displaySubstitute, powerTubeSubstitute);

            ui = new UserInterface(
                powerButtonSubstitute,
                timeButtonSubstitute,
                startCancelButtonSubstitute,
                doorSubstitute,
                displaySubstitute,
                lightSubstitute,
                uut);

            uut.UI = ui;
        }

        [Test]
        public void StartCancelPressedRunsForAFewSeconds_IsInCorrectState_DisplayShowTimeCalled()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();


            //Boilerplate to force a wait until ShowTime is called
            AutoResetEvent reset = new AutoResetEvent(false);

            displaySubstitute
                .When(d => d.ShowTime(Arg.Any<int>(), Arg.Any<int>()))
                .Do(c => reset.Set());


            //Wait until function called
            reset.WaitOne(1500);

            displaySubstitute.Received().ShowTime(1, 0);
        }

        /*
         * These tests take a full minute, very long for a test.
         * We chose to do this, under the assumption that these integration
         * tests would be run as a part of a nightly build, so time isn't as much of
         * a factor as with unit tests. The length is necessary to ensure that the
         * system interacts properly - It helped us spot an error!
         *
         * If we didn't wait a minute (the minimum time that can be set on the microwave)
         * it's possible that it would never actually finish!
         */

        [Test]
        public void StartCancelPressedRunsForAMinute_IsInCorrectState_DisplayShowTimeCalled61Times()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Boilerplate to force a wait until ShowTime is called
            AutoResetEvent reset = new AutoResetEvent(false);
            int eventCount = 0;


            displaySubstitute
                .When(d => d.ShowTime(Arg.Any<int>(), Arg.Any<int>()))
                .Do(c =>
                {
                    if (eventCount < 60)
                    {
                        eventCount++;
                    }
                    else
                    {
                        reset.Set();
                    }
                });

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();


            


            //Wait up to a minute
            reset.WaitOne(61000);

            displaySubstitute.ReceivedWithAnyArgs(61).ShowTime(default, default);
        }


        //Normally, we'd also test that Display was cleared, and Light turned off
        //Since the test takes so long, we only test PowerTube - We tested that the rest turns off in step 1 anyway
        //It's then assumed that if PowerTube turned off, the rest must have too.
        [Test]
        public void StartCancelPressedRunsForAMinute_IsInCorrectState_TurnsOffPowerState()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Boilerplate to force a wait until ShowTime is called
            AutoResetEvent reset = new AutoResetEvent(false);

            powerTubeSubstitute
                .When(d => d.TurnOff())
                .Do(c =>
                {
                    reset.Set();
                });


            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();


            

            //Wait up to a minute
            reset.WaitOne(61000);

            powerTubeSubstitute.Received().TurnOff();
        }


        //Testing that the Timer is stopped is very internal and whitebox
        //- So instead we test that Display doesn't receive any more calls! 
        // That's external to the current SUT, so not whitebox.
        // If the system is stopped, the display shouldn't be showing anything new!

        [Test]
        public void StartCancelPressedWhileCooking_HasCookedFor2Seconds_DisplayReceivesNoMoreCallsAfterBeingStopped()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Boilerplate to force a wait until ShowTime is called
            // 1:00 - 0.59 - 0.58 <- Should receive 3 calls total
            // Stop startcancel press is called on 3rd call - should receive no more!
            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);

            displaySubstitute
                .When(d => d.ShowTime(Arg.Any<int>(), Arg.Any<int>()))
                .Do(c =>
                {
                    if (count < 2)
                    {
                        count++;
                    }
                    else if (count == 2)
                    {
                        startCancelButtonSubstitute.Pressed += Raise.Event();
                    }
                    else
                    {
                        reset.Set();
                    }
                });


            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //If interrupted, that means we counted once too much!
            //Wait for long enough to be fairly certain no more calls could be received.
            Assert.False(reset.WaitOne(5000));

            displaySubstitute.Received(3);
        }

        [Test]
        public void DoorOpenedWhileCooking_HasCookedFor2Seconds_DisplayReceivesNoMoreCallsAfterBeingStopped()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Boilerplate to force a wait until ShowTime is called
            // 1:00 - 0.59 - 0.58 <- Should receive 3 calls total
            // Stop startcancel press is called on 3rd call - should receive no more!
            int count = 0;
            AutoResetEvent reset = new AutoResetEvent(false);

            displaySubstitute
                .When(d => d.ShowTime(Arg.Any<int>(), Arg.Any<int>()))
                .Do(c =>
                {
                    if (count < 2)
                    {
                        count++;
                    }
                    else if (count == 2)
                    {
                        doorSubstitute.Opened += Raise.Event();
                    }
                    else
                    {
                        reset.Set();
                    }
                });


            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();


            //If interrupted, that means we counted once too much!
            //Wait for long enough to be fairly certain no more calls could be received.
            Assert.False(reset.WaitOne(5000));

            displaySubstitute.Received(3);
        }

    }
}