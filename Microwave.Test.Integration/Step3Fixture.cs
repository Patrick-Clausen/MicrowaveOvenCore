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
    public class Step3Fixture
    {
        //SUBSTITUTES
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
        private IDoor door;

        [SetUp]
        public void Setup()
        {
            powerButtonSubstitute = Substitute.For<IButton>();
            timeButtonSubstitute = Substitute.For<IButton>();
            startCancelButtonSubstitute = Substitute.For<IButton>();
            powerTubeSubstitute = Substitute.For<IPowerTube>();
            displaySubstitute = Substitute.For<IDisplay>();
            lightSubstitute = Substitute.For<ILight>();

            door = new Door();
            timer = new Timer();

            uut = new CookController(timer, displaySubstitute, powerTubeSubstitute);

            ui = new UserInterface(
                powerButtonSubstitute,
                timeButtonSubstitute,
                startCancelButtonSubstitute,
                door,
                displaySubstitute,
                lightSubstitute,
                uut);

            uut.UI = ui;
        }



        /*
         * First two interactions;
         * Whenever the door opens, the light should turn on - When it closes, the light should turn off.
         * If the door closes while it's closed - Nothing should happen.
         */
        [Test]
        public void DoorOpen_IsInReadyState_LightTurnsOn()
        {
            //ARRANGE
            //Already in ready state by default

            //ACT
            door.Open();
            
            //ASSERT
            lightSubstitute.Received().TurnOn();
        }

        [Test]
        public void DoorClose_IsInDoorOpenState_LightTurnsOff()
        {
            //ARRANGE
            door.Open(); //Door needs to be open to close it..

            //ACT
            door.Close();

            lightSubstitute.Received().TurnOff();
        }


        /*
         * Now for interactions only defined in STM for some reason - When we are in SetPower or SetTime states,
         * we need to reset values, clear display and turn on light.
         *
         * Checking the values would be fairly whitebox, but we can just test on the state
         * If start-cancel is pressed while time is set, cooking should start - That shouldn't happen if the door has been opened.
         */

        [Test]
        public void DoorOpen_PowerSetTo100_DisplayIsCleared()
        {
            //ARRANGE
            //Press power button twice, should now be running at 100 instead of fifty.
            powerButtonSubstitute.Pressed += Raise.Event();
            powerButtonSubstitute.Pressed += Raise.Event();

            //ACT
            door.Open();

            //ASSERT
            displaySubstitute.Received().Clear();
        }

        [Test]
        public void DoorOpen_PowerSetTo100_LightTurnsOn()
        {
            //ARRANGE
            //Press power button twice, should now be running at 100 instead of fifty.
            powerButtonSubstitute.Pressed += Raise.Event();
            powerButtonSubstitute.Pressed += Raise.Event();

            //ACT
            door.Open();

            //ASSERT
            lightSubstitute.Received().TurnOn();
        }

        [Test]
        public void DoorOpen_PowerSetTo100AndStarted_DoesntRun()
        {
            //ARRANGE
            //Press power button twice, should now be running at 100 instead of fifty.
            powerButtonSubstitute.Pressed += Raise.Event();
            powerButtonSubstitute.Pressed += Raise.Event();

            //Press time button to get into SetTime state
            timeButtonSubstitute.Pressed += Raise.Event();



            //ACT
            //Open and close the door, should now be in ready instead of SetTime state
            door.Open();
            door.Close();

            //Press start-cancel -> Should do nothing
            startCancelButtonSubstitute.Pressed += Raise.Event();


            //ASSERT
            //PowerTube shouldn't turn on, it only would if the power was turned on.
            powerTubeSubstitute.DidNotReceiveWithAnyArgs().TurnOn(default);
        }

        [Test]
        public void DoorOpen_TimeSetToTwoMinutes_DisplayIsCleared()
        {
            //ARRANGE
            //Must be in SetPower state to set time
            powerButtonSubstitute.Pressed += Raise.Event();
            //Press time button twice, to set to two minutes
            timeButtonSubstitute.Pressed += Raise.Event();
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            door.Open();

            //ASSERT
            displaySubstitute.Received().Clear();
        }

        [Test]
        public void DoorOpen_TimeSetToTwoMinutes_LightTurnsOn()
        {
            //ARRANGE
            //Must be in SetPower state to set time
            powerButtonSubstitute.Pressed += Raise.Event();
            //Press time button twice, to set to two minutes
            timeButtonSubstitute.Pressed += Raise.Event();
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            door.Open();

            //ASSERT
            lightSubstitute.Received().TurnOn();
        }

        /*
         * Final interaction - Door is opened while cooking ->
         * Stop cooking
         */

        [Test]
        public void DoorOpen_IsCooking_StopsCooking()
        {
            //ARRANGE
            //Must be in SetPower state to set time
            powerButtonSubstitute.Pressed += Raise.Event();
            //Press time button twice, to get to SetTime state
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Boilerplate to force a wait until ShowTime is called
            //Using ShowTime to enforce runtime - is called once a second
            //Thereby, if called 4 times -> 3 seconds(+1 for initial time)
            //If door is opened after this, cooking should be interrupted.
            AutoResetEvent reset = new AutoResetEvent(false);
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
                        door.Open();
                    }
                });
            
            //Break timer when turned off
            powerTubeSubstitute
                .When(p => p.TurnOff())
                .Do(c => reset.Set());

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //Now wait!
            reset.WaitOne(6000);

            //ASSERT
            powerTubeSubstitute.Received().TurnOff();
        }
    }
}