import React, { Component } from 'react';
import { withStyles } from '@material-ui/core/styles';
import Button from '@material-ui/core/Button';
import Typography from '@material-ui/core/Typography';
import {inject, observer} from 'mobx-react';

const styles = theme => ({
 
  input: {
    display: 'none',
  },
});

@inject("appStore")
@observer
class Home extends Component {
  displayName = Home.name

  componentDidMount = async () => {
      await this.props.appStore.setsGetAll();
      await this.props.appStore.settingsGetAll();
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
            onClick={() => this.props.appStore.executionRestart()}
          >
            RESTART SHOW
          </Button>

          <Button variant="outlined"
            color="primary"
            className="quickSelect"
            onClick={ () => async () => {
              await this.props.appStore.executionSet(0);
              await this.props.appStore.executionRestart();
            }}
          >
            STOP EXECUTION
          </Button>

          <Button variant="outlined"
            color="primary"
            className="quickSelect"
            onClick={() => this.props.appStore.executionCurrentOnly()}
          >
            START EXECUTION
          </Button>

          <Button variant="outlined"
            color="primary"
            className="quickSelect"
            onClick={() => this.props.appStore.executionRandom()}
          >
            START RANDOM
          </Button>

           {this.props.appStore.sets.map((set, i) =>
                                    (
                                      <Button variant="outlined"
                                      color={this.props.appStore.currentSet === set.setId ? 
                                        'secondary' : 'primary'
                                      }
                                      onClick={() =>this.props.appStore.currentSetSet(set.setId)}
                                      key={i}
                                    >
                                      {set.setName}
                                    </Button>
                                    ))}

        </div>
      </div>
    );
  }
}

export default withStyles(styles)(Home);