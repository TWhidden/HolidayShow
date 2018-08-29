import React, { Component } from 'react';
import { Link } from 'react-router-dom';
// import { Glyphicon, Nav, Navbar, NavItem } from 'react-bootstrap';
// import { LinkContainer } from 'react-router-bootstrap';
import { ListItem } from '@material-ui/core';
import List from '@material-ui/core/List';
import ListItemIcon from '@material-ui/core/ListItemIcon';
import ListItemText from '@material-ui/core/ListItemText';

// ICONS FOUND HERE: https://material.io/tools/icons/?style=baseline

import SettingsIcon from '@material-ui/icons/SettingsOutlined';
import ComputerIcon from '@material-ui/icons/Computer';
import PatternIcon from '@material-ui/icons/Reorder';
import SetIcon from '@material-ui/icons/PlaylistAdd';
import EffectIcon from '@material-ui/icons/LowPriority';

import './NavMenu.css';

export class NavMenu extends Component {
  displayName = NavMenu.name

  render() {
    return (

      <List disablePadding style={{ width: "224px" }}>
        <ListItem button component={Link} to={`/DeviceEditor/`} >
          <ListItemIcon>
            <ComputerIcon />
          </ListItemIcon>
          <ListItemText primary="Devices" /></ListItem>

        <ListItem button component={Link} to={`/DevicePatternEditor/`} >
          <ListItemIcon>
            <PatternIcon />
          </ListItemIcon>
          <ListItemText primary="Device Patterns" /></ListItem>

        <ListItem button component={Link} to={`/SetsEditor/`} >
          <ListItemIcon>
            <SetIcon />
          </ListItemIcon>
          <ListItemText primary="Sets" /></ListItem>

        <ListItem button component={Link} to={`/EffectsEditor/`} >
          <ListItemIcon>
            <EffectIcon />
          </ListItemIcon>
          <ListItemText primary="Effects" /></ListItem>

        <ListItem button component={Link} to={`/SettingsEditor/`} >
          <ListItemIcon>
            <SettingsIcon />
          </ListItemIcon>
          <ListItemText primary="Settings" /></ListItem>
      </List>


      // <Navbar inverse fixedTop fluid collapseOnSelect>
      //   {/* <Navbar.Header>
      //     <Navbar.Brand>
      //       <Link to={'/'}>Holiday Show Editor</Link>
      //     </Navbar.Brand> 
      //     <Navbar.Toggle />
      //   </Navbar.Header> */}
      //   <Navbar.Collapse>
      //     <Nav>
      //       {/* <LinkContainer to={'/'} exact>
      //         <NavItem>
      //           <Glyphicon glyph='home' /> Home
      //         </NavItem>
      //       </LinkContainer> */}
      //       <LinkContainer to={'/DeviceEditor'} exact>
      //         <NavItem>
      //           <Glyphicon glyph='lamp' />Devices
      //         </NavItem>
      //       </LinkContainer>
      //       <LinkContainer to={'/DevicePatternEditor'} exact>
      //         <NavItem>
      //           <Glyphicon glyph='align-center' />Device Patterns
      //         </NavItem>
      //       </LinkContainer>
      //       <LinkContainer to={'/SetsEditor'} exact>
      //         <NavItem>
      //           <Glyphicon glyph='film' />Sets
      //         </NavItem>
      //       </LinkContainer>
      //       <LinkContainer to={'/EffectsEditor'} exact>
      //         <NavItem>
      //           <Glyphicon glyph='pencil' />Effects
      //         </NavItem>
      //       </LinkContainer>
      //       <LinkContainer to={'/SettingsEditor'} exact>
      //         <NavItem>
      //           <Glyphicon glyph='cog' />Settings
      //         </NavItem>
      //       </LinkContainer>
      //     </Nav>
      //   </Navbar.Collapse>
      // </Navbar>
    );
  }
}
