// NavMenu.tsx
import { FC } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import ListItemButton from '@mui/material/ListItemButton';

import SettingsIcon from '@mui/icons-material/SettingsOutlined';
import ComputerIcon from '@mui/icons-material/Computer';
import PatternIcon from '@mui/icons-material/Reorder';
import SetIcon from '@mui/icons-material/PlaylistAdd';
import EffectIcon from '@mui/icons-material/LowPriority';
import HomeIcon from '@mui/icons-material/Home';

interface NavMenuProps {
  open: boolean;
}

const NavMenu: FC<NavMenuProps> = ({ open }) => {
  const navItems = [
    { text: 'Home', icon: <HomeIcon />, path: '/' },
    { text: 'Devices', icon: <ComputerIcon />, path: '/DeviceEditor' },
    { text: 'Device Patterns', icon: <PatternIcon />, path: '/DevicePatternEditor' },
    { text: 'Sets', icon: <SetIcon />, path: '/SetsEditor' },
    { text: 'Effects', icon: <EffectIcon />, path: '/EffectsEditor' },
    { text: 'Settings', icon: <SettingsIcon />, path: '/SettingsEditor' },
  ];

  return (
    <List disablePadding>
      {navItems.map((item) => (
        <ListItem key={item.text} disablePadding>
          <ListItemButton component={RouterLink} to={item.path}>
            <ListItemIcon>{item.icon}</ListItemIcon>
            {open && <ListItemText primary={item.text} />}
          </ListItemButton>
        </ListItem>
      ))}
    </List>
  );
};

export default NavMenu;
