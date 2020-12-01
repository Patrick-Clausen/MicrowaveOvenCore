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
    public class Step5Fixture
    {
        //SUBSTITUTES
        private IDisplay displaySubstitute;
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

        [SetUp]
        public void Setup()
        {

            displaySubstitute = Substitute.For<IDisplay>();
            lightSubstitute = Substitute.For<ILight>();
            outputSubstitute = Substitute.For<IOutput>();

            powerTube = new PowerTube(outputSubstitute);

            powerButton = new Button();
            timeButton = new Button();
            startCancelButton = new Button();
            door = new Door();
            timer = new Timer();

            uut = new CookController(timer, displaySubstitute, powerTube);

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

        [Test]
        public void StartCancelPressed_IsInSetTimeState_PowerTubeCallsOutPutLogLineTurnOn()
        {
            //ARRANGE
            powerButton.Press();
            timeButton.Press();
            //ACT
            startCancelButton.Press();

            //ASSERT
            outputSubstitute.Received().OutputLine(Arg.Is<string>(str => str.Contains("PowerTube works with")));
        }

        [Test]
        public void StartCancelPressed_IsInCookingState_PowerTubeCallsOutPutLogLineTurnoff()
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
            outputSubstitute.Received().OutputLine("PowerTube turned off");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void StartCancelPressed_IsInCookingState_PowerIsCorrectAndLoggedToOutput(int expectedPower)
        {

            //ARRANGE
            for (int i = 0; i < expectedPower; i++)
                powerButton.Press();

            int expectedPowerRanger = expectedPower * 50;
            timeButton.Press();
            //ACT
            startCancelButton.Press();
            //ASSERT
            outputSubstitute.Received().OutputLine(Arg.Is<string>(str => str.Contains($"{expectedPowerRanger}")));
        }

        [Test]
        public void TimerRunsOut_IsInCookingState_PowerTubeTurnsOffAndCallsOutPut()
        {
            //Arrange
            powerButton.Press();
            timeButton.Press();
            AutoResetEvent reset = new AutoResetEvent(false);
            //ACT
            startCancelButton.Press();

            //Wait for cooking to finish
            reset.WaitOne(61000);

            //ASSERT
            outputSubstitute.Received().OutputLine("PowerTube turned off");
        }

        [Test]
        public void DoorOpen_IsInCookingState_PowerTubeCallsOutPutLogLineTurnoff()
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
            outputSubstitute.Received().OutputLine("PowerTube turned off");
        }
    }
        
}