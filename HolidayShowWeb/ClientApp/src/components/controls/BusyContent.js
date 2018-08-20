import React, { Component } from 'react';
import CircularProgress from '@material-ui/core/CircularProgress'

export default class BusyContent extends Component {

    constructor(props) {
        super(props)

        this.state = {

        }
    }

    render() {
        return (
            <div className="dimmed">
            <div className="centerMiddleContent">
                <CircularProgress ref={this.setProgressRef}/>
            </div>
        </div>
        )
    }

}