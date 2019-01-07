
// Import default styles.
// This only needs to be done once; probably during bootstrapping process.
import './index.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import registerServiceWorker from './registerServiceWorker';
import AppStore from './Stores/appStore';
import { Provider } from 'mobx-react';
import BusyContent from './components/controls/BusyContent';
import ErrorContent from './components/controls/ErrorContent';
import {observer} from 'mobx-react'


const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

const appStore = new AppStore();

@observer
class RenderBusyAndError extends React.Component 
{
  render(){
    return (
    <>
      {
        appStore.isBusy && (<BusyContent />)
      }
      <ErrorContent errorMessage={appStore.errorMessage} errorClear={()=>{appStore.errorMessageSet(null)}}/>
      </>
    )
  }
}

ReactDOM.render(
  <Provider appStore={appStore}>
    <BrowserRouter basename={baseUrl}>
      <>
        <App />
        <RenderBusyAndError/>    
      </>
    </BrowserRouter>
  </Provider>,
  rootElement);

registerServiceWorker();
