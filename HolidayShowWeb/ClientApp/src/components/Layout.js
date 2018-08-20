import React, { Component } from 'react';
import { Col, Grid, Row } from 'react-bootstrap';
import { NavMenu } from './NavMenu';
import CssBaseline from '@material-ui/core/CssBaseline';

export class Layout extends Component {
  displayName = Layout.name

  render() {
    return (
      <React.Fragment>
      <CssBaseline/>
      <Grid fluid>
        <Row>
          <Col sm={3}>
            <NavMenu />
          </Col>
          <Col sm={9}>
            {this.props.children}
          </Col>
        </Row>
      </Grid>
      </React.Fragment>
    );
  }
}
