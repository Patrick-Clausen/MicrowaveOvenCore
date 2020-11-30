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
    public class Step7Fixture
    {
        //SUBSTITUTES
        private IOutput outputSubstitute;

        //SUT
        private ILight light;
        private CookController uut;
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
            outputSubstitute = Substitute.For<IOutput>();

            powerTube = new PowerTube(outputSubstitute);
            display = new Display(outputSubstitute);
            light = new Light(outputSubstitute);

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
        }

        [Test]
        public void DoorIsOpened_IsInReadyState_LightCallsOutputLogLine()
        {
            //ARRANGE
            //Already in ready state by default

            //ACT
            door.Open();

            //ASSERT
            outputSubstitute.ReceivedWithAnyArgs().OutputLine(Arg.Any<string>());
        }

        [Test]
        public void DoorIsClosed_IsInDoorIsOpenState_LightCallsOutputLogLineTwice()
        {
            //ARRANGE
            door.Open();

            //ACT
            door.Close();

            //ASSERT
            outputSubstitute.ReceivedWithAnyArgs(2).OutputLine(Arg.Any<string>());
        }

        [Test]
        public void DoorIsOpened_IsInSetPowerState_LightCallsOutputLogLine()
        {
            //ARRANGE
            powerButton.Press();

            //ACT
            door.Open();

            //ASSERT
            outputSubstitute.ReceivedWithAnyArgs().OutputLine(Arg.Any<string>());
        }

        [Test]
        public void DoorIsOpened_IsInSetTimeState_LightCallsOutputLogLine()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();

            //ACT
            door.Open();

            //ASSERT
            outputSubstitute.ReceivedWithAnyArgs().OutputLine(Arg.Any<string>());
        }

        [Test]
        public void StartCancelButtonIsPressed_IsInCookingState_LightCallsOutputLogLineWithTurnOnAndThenTurnOff()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);

            //ACT
            startCancelButton.Press();

            //Wait for cooking to finish
            reset.WaitOne(61000);

            //ASSERT
            outputSubstitute.Received().OutputLine("Light is turned on");
            outputSubstitute.Received().OutputLine("Light is turned off");
        }


        [Test]
        public void StartCancelButtonIsPressed_IsInCookingStateStartCancelButtonIsPressed_LightCallsOutputLogLineWithTurnOff()
        {
            //ARRANGE
            string expected = "Light is turned off";
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);


            //ACT
            startCancelButton.Press();
            //Wait a second for good measure
            reset.WaitOne(1000);
            startCancelButton.Press();

            //ASSERT
            outputSubstitute.Received().OutputLine(expected);
        }
    }
}