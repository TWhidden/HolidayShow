import React, { Component } from 'react';
import { Route } from 'react-router';
import Layout from './components/Layout';
import Home from './components/Home';
import DeviceManager from './components/DeviceEditor';
import SetsEditor from './components/SetsEditor';
import EffectsEditor from './components/EffectsEditor';
import SettingsEditor from './components/SettingsEditor';
import DevicePatternEditor from './components/DevicePatternEditor';

export default class App extends Component {
  displayName = App.name

  render() {
    return (

        <Layout>
          {/* <CssBaseline /> */}
          <Route exact path='/' component={Home} />
          <Route exact path='/DeviceEditor' component={DeviceManager} />
          <Route exact path='/DevicePatternEditor' component={DevicePatternEditor} />
          <Route exact path='/SetsEditor' component={SetsEditor} />
          <Route exact path='/EffectsEditor' component={EffectsEditor} />
          <Route exact path='/SettingsEditor' component={SettingsEditor} />
        </Layout>

    );
  }
}
