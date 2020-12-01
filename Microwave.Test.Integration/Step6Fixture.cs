using NUnit.Framework;
using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using Microwave.Classes.Boundary;
using NSubstitute;
using Timer = Microwave.Classes.Boundary.Timer;
using System.Threading;

namespace Microwave.Test.Integration
{
    [TestFixture]
    public class Step6Fixture
    {
        //SUBSTITUTES
        private ILight lightSubstitute;
        private IOutput outputSubstitute;

        //SUT
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
            lightSubstitute = Substitute.For<ILight>();
            outputSubstitute = Substitute.For<IOutput>();

            display = new Display(outputSubstitute);
            powerTube = new PowerTube(outputSubstitute);

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
                lightSubstitute,
                uut);

            uut.UI = ui;
        }

        [Test]
        public void ShowPowerOnPowerPressed_IsInReadyState_DisplayCallsOutPutOutLine()
        {
            //ARRANGE
            //Is arranged

            //ACT
            powerButton.Press();
            //ASSERT
            outputSubstitute.Received().OutputLine(Arg.Is<string>(str => str.Contains($"Display shows:")));
        }

        [Test]
        public void ShowTimeOnTimePressed_IsInSetPowerState_DisplayCallsOutPutOutLine()
        {
            //ARRANGE
            powerButton.Press();

            //ACT
            timeButton.Press();

            //ASSERT
            outputSubstitute.Received().OutputLine(Arg.Is<string>(str => str.Contains($"Display shows: 01:00")));
        }

        [Test]
        public void ShowTimeOnTimerTick_IsInCookingState_DisplayCallsOutPutOutLine()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);

            //ACT
            startCancelButton.Press();

            //Wait for cooking to finish
            reset.WaitOne(16000);

            //ASSERT
            //Checking for time value between input and 30.
            outputSubstitute.Received().OutputLine(Arg.Is<string>(str => str.Contains($"Display shows: 00:50")));
        }

        [Test]
        public void ClearDisplayOnFinishCooking_IsInCookingState_DisplayCallsOutPutOutLine()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);


            //ACT
            startCancelButton.Press();
            reset.WaitOne(61000);

            //ASSERT
            outputSubstitute.Received().OutputLine("Display cleared");
        }

        [Test]
        public void ClearDisplayOnDoorOpened_IsInCookingState_DisplayCallsOutPutOutLine()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);

            //ACT
            startCancelButton.Press();
            reset.WaitOne(1000);
            door.Open();

            //ASSERT
            outputSubstitute.Received().OutputLine("Display cleared");
        }
        [Test]
        public void ClearDisplayOnStartCancelPressed_IsInCookingState_DisplayCallsOutPutOutLine()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);

            //ACT
            startCancelButton.Press();
            reset.WaitOne(1000);
            startCancelButton.Press();

            //ASSERT
            outputSubstitute.Received().OutputLine("Display cleared");
        }

    }
}