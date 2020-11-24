using Microwave.Classes.Controllers;
using Microwave.Classes.Interfaces;
using NSubstitute;
using NUnit.Framework;

namespace Microwave.Test.Integration
{
    [TestFixture]
    public class Step1Fixture
    {
        //SUBSTITUTES
        private ITimer timerSubstitute;
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

        [SetUp]
        public void Setup()
        {
            timerSubstitute = Substitute.For<ITimer>();
            doorSubstitute = Substitute.For<IDoor>();
            powerButtonSubstitute = Substitute.For<IButton>();
            timeButtonSubstitute = Substitute.For<IButton>();
            startCancelButtonSubstitute = Substitute.For<IButton>();
            powerTubeSubstitute = Substitute.For<IPowerTube>();
            displaySubstitute = Substitute.For<IDisplay>();
            lightSubstitute = Substitute.For<ILight>();

            uut = new CookController(timerSubstitute, displaySubstitute, powerTubeSubstitute);

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

        #region StartCooking
        [Test]
        public void SUTReceivesOnStartCancelPressedEvent_IsInCorrectState_StartTimerIsCalled()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Press startCancelButton to act on SUT
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            timerSubstitute.Received().Start(60);
        }

        [Test]
        public void SUTReceivesOnStartCancelPressedEvent_IsInCorrectState_TurnOnPowerTubeIsCalled()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Press startCancelButton to act on SUT
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            powerTubeSubstitute.TurnOn(50);
        }

        [Test]
        public void SUTReceivesOnStartCancelPressedEvent_IsInIncorrectState_StartTimerIsNotCalled()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Don't press timer, so isn't ready to start cooking

            //ACT
            //Press startCancelButton to act on SUT
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            timerSubstitute.DidNotReceiveWithAnyArgs().Start(default);
        }

        [Test]
        public void SUTReceivesOnStartCancelPressedEvent_IsInIncorrectState_TurnOnPowerTubeIsNotCalled()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Don't press timer, so isn't ready to start cooking

            //ACT
            //Press startCancelButton to act on SUT
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            powerTubeSubstitute.DidNotReceiveWithAnyArgs().TurnOn(default);
        }
        #endregion

        #region CookingIsDone
        [Test]
        public void SUTReceivesOnTimerExpired_IsInCorrectState_UICallsDisplayClear()
        {
            //ARRANGE
            //Get system into correct state
           
            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, press startcancelbutton to get into cooking state
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //To get UUT into cooking state
            uut.StartCooking(50,60);

            //ACT
            //Press startCancelButton to act on SUT
            timerSubstitute.Expired += Raise.Event();

            //ASSERT
            displaySubstitute.Received().Clear();
        }

        [Test]
        public void SUTReceivesOnTimerExpired_IsInCorrectState_LightReceivesTurnOff()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, press startcancelbutton to get into cooking state
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //To get UUT into cooking state
            uut.StartCooking(50, 60);

            //ACT
            //Press startCancelButton to act on SUT
            timerSubstitute.Expired += Raise.Event();

            //ASSERT
            lightSubstitute.Received().TurnOff();
        }

        [Test]
        public void SUTReceivesOnTimerExpired_IsInCorrectState_UIDoesNotCallDisplayClear()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //To get UUT into cooking state
            uut.StartCooking(50, 60);

            //ACT
            //Press startCancelButton to act on SUT
            timerSubstitute.Expired += Raise.Event();

            //ASSERT
            displaySubstitute.DidNotReceive().Clear();
        }

        [Test]
        public void SUTReceivesOnTimerExpired_IsInIncorrectState_LightDoesNotReceiveTurnOff()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //To get UUT into cooking state
            uut.StartCooking(50, 60);

            //ACT
            //Press startCancelButton to act on SUT
            timerSubstitute.Expired += Raise.Event();

            //ASSERT
            lightSubstitute.DidNotReceive().TurnOff();
        }
        #endregion

        #region DoorOpenedWhileCooking
        [Test]
        public void SUTReceivesOnDoorOpened_IsCooking_TimerReceivesStop()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Open door
            doorSubstitute.Opened += Raise.Event();

            //ASSERT
            timerSubstitute.Received().Stop();
        }

        [Test]
        public void SUTReceivesOnDoorOpened_IsCooking_PowerTubeReceivesTurnOff()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Open door
            doorSubstitute.Opened += Raise.Event();

            //ASSERT
            powerTubeSubstitute.Received().TurnOff();
        }

        [Test]
        public void SUTReceivesOnDoorOpened_IsNotCooking_TimerDoesNotReceiveStop()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Open door
            doorSubstitute.Opened += Raise.Event();

            //ASSERT
            timerSubstitute.DidNotReceive().Stop();
        }

        [Test]
        public void SUTReceivesOnDoorOpened_IsNotCooking_PowerTubeDoesNotReceiveTurnOff()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Open door
            doorSubstitute.Opened += Raise.Event();

            //ASSERT
            powerTubeSubstitute.DidNotReceive().TurnOff();
        }
        #endregion

        #region StartCancelPressedWhileCooking
        [Test]
        public void SUTReceivesStartCancelPressed_IsCooking_TimerReceivesStop()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Start-cancel pressed
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            timerSubstitute.Received().Stop();
        }

        [Test]
        public void SUTReceivesStartCancelPressed_IsCooking_PowerTubeReceivesTurnOff()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //Third, start-cancel button to start cooking
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Start-cancel pressed
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            powerTubeSubstitute.Received().TurnOff();
        }

        [Test]
        public void SUTReceivesStartCancelPressed_IsNotCooking_TimerDoesNotReceiveStop()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Start-cancel pressed
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            timerSubstitute.DidNotReceive().Stop();
        }

        [Test]
        public void SUTReceivesStartCancelPressed_IsNotCooking_PowerTubeDoesNotReceiveTurnOff()
        {
            //ARRANGE
            //Get system into correct state

            //First, press power button to set powerlevel
            powerButtonSubstitute.Pressed += Raise.Event();

            //Second, press time button to set time
            timeButtonSubstitute.Pressed += Raise.Event();

            //ACT
            //Start-cancel pressed
            startCancelButtonSubstitute.Pressed += Raise.Event();

            //ASSERT
            powerTubeSubstitute.DidNotReceive().TurnOff();
        }
        #endregion
    }
}