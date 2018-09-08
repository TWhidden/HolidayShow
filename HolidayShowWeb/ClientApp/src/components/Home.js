import React, { Component } from 'react';

import SettingServices from '../Services/SettingServices';
import SetServices from '../Services/SetServices';

import { withStyles } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import BusyContent from './controls/BusyContent';
import ErrorContent from './controls/ErrorContent';
import * as Enumerable from "linq-es2015";

const styles = theme => ({
  button: {
    margin: theme.spacing.unit,
  },
  input: {
    display: 'none',
  },
});

export class Home extends Component {
  displayName = Home.name

  constructor(props) {
    super(props)

    this.SettingServices = SettingServices;
    this.SetServices = SetServices;

    this.state = ({
      isBusy: false,
      sets: [],
      settings: [],
      errorMessage: null,
    })
  }

  componentDidMount = async () => {
    try {
      this.setIsBusy(true);

      await this.SettingServices.executionOff();
      
      let sets = await this.SetServices.getAllSets();
      let settings = await this.SettingServices.getAllSettings();

      this.setState({
        sets, 
        settings,
      });
    } catch (e) {
      this.setState({errorMessage: e.message})
    } finally {
      this.setIsBusy(false);
    }
  }

  handleStopExecution = async () => {
    try {
      this.setIsBusy(true);

      await this.SettingServices.executionOff();
      await this.SettingServices.executionRestart();

      this.setState({});
    } catch (e) {
      this.setState({errorMessage: e.message})
    } finally {
      this.setIsBusy(false);
    }
  }

  handleRestartExecution = async () => {
    try {
      this.setIsBusy(true);

      await this.SettingServices.executionRestart();

      this.setState({});
    } catch (e) {
      this.setState({errorMessage: e.message})
    } finally {
      this.setIsBusy(false);
    }
  }

  handleStartExecution = async () => {
    try {
      this.setIsBusy(true);

      await this.SettingServices.executionCurrentOnly();

      this.setState({});
    } catch (e) {
      this.setState({errorMessage: e.message})
    } finally {
      this.setIsBusy(false);
    }
  }

  handleStartSet = async (setId) => {
    try {
      this.setIsBusy(true);

      // get current setting, 
      var setting = Enumerable.asEnumerable(this.state.settings)
                    .Where(setting => setting.settingName === "CurrentSet")
                    .FirstOrDefault();

      if(setting == null){
          setting = {
            settingName: "CurrentSet",
            valueDouble: setId
          }
          setting = await this.SettingServices.createSetting(setting);
          let settings = this.state.settings;
          settings.push(setting);

            this.setState({
              settings
            });
      }else{
        setting.valueDouble = setId;
        await this.SettingServices.saveSetting(setting)
      }

      await this.SettingServices.executionCurrentOnly();

      this.setState({});
    } catch (e) {
      this.setState({errorMessage: e.message})
    } finally {
      this.setIsBusy(false);
    }
  }

  setIsBusy(busyState) {
    clearTimeout(this.timer);
    if (!busyState) {
        this.setState({ isBusy: false });
        return;
    }

    this.timer = setTimeout(() => this.setState({ isBusy: true }), 250);
}

  render() {

    return (
      <div>
        <div >
          <Typography variant="body2" gutterBottom>
            Welcome to the holiday show.  This page will allow you to quickly control the pre-programed sequences.
          </Typography>


        </div>
        <div className="quickSelectContainer">

          <Button variant="outlined"
            color="primary"
            className="quickSelect"
            margin="10"
            onClick={() => this.handleRestartExecution()}
          >
            RESTART SHOW
          </Button>

          <Button variant="outlined"
            color="primary"
            className="quickSelect"
            onClick={() => this.handleStopExecution()}
          >
            STOP EXECUTION
          </Button>

          <Button variant="outlined"
            color="primary"
            className="quickSelect"
            onClick={() => this.handleStartExecution()}
          >
            START EXECUTION
          </Button>

           {this.state.sets.map((set, i) =>
                                    (
                                      <Button variant="outlined"
                                      color="primary"
                                      className="quickSelect"
                                      onClick={() => this.handleStartSet(set.setId)}
                                      key={i}
                                    >
                                      {set.setName}
                                    </Button>
                                    ))}

        </div>

        {
          this.state.isBusy && (<BusyContent />)
        }
        <ErrorContent errorMessage={this.state.errorMessage} errorClear={()=>{this.setState({errorMessage: null})}}/>
      </div>
    );
  }
}
export default withStyles(styles)(Home);