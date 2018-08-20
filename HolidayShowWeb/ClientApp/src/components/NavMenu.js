import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { Glyphicon, Nav, Navbar, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import './NavMenu.css';

export class NavMenu extends Component {
  displayName = NavMenu.name

  render() {
    return (
      <Navbar inverse fixedTop fluid collapseOnSelect>
        <Navbar.Header>
          <Navbar.Brand>
            <Link to={'/'}>Holiday Show Editor</Link>
          </Navbar.Brand>
          <Navbar.Toggle />
        </Navbar.Header>
        <Navbar.Collapse>
          <Nav>
            {/* <LinkContainer to={'/'} exact>
              <NavItem>
                <Glyphicon glyph='home' /> Home
              </NavItem>
            </LinkContainer> */}
            <LinkContainer to={'/DeviceEditor'} exact>
              <NavItem>
                <Glyphicon glyph='lamp' />Devices
              </NavItem>
            </LinkContainer>
            <LinkContainer to={'/DevicePatternEditor'} exact>
              <NavItem>
                <Glyphicon glyph='align-center' />Device Patterns
              </NavItem>
            </LinkContainer>
            <LinkContainer to={'/SetsEditor'} exact>
              <NavItem>
                <Glyphicon glyph='film' />Sets
              </NavItem>
            </LinkContainer>
            <LinkContainer to={'/EffectsEditor'} exact>
              <NavItem>
                <Glyphicon glyph='pencil' />Effects
              </NavItem>
            </LinkContainer>
            <LinkContainer to={'/SettingsEditor'} exact>
              <NavItem>
                <Glyphicon glyph='cog' />Settings
              </NavItem>
            </LinkContainer>
          </Nav>
        </Navbar.Collapse>
      </Navbar>
    );
  }
}
