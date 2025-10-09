// Layout.tsx
import React, { useState, useEffect, FC } from 'react';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Drawer,
  CssBaseline,
  useTheme,
  useMediaQuery,
  styled,
  Theme,
  Box,
  CircularProgress,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import Brightness4Icon from '@mui/icons-material/Brightness4';
import Brightness7Icon from '@mui/icons-material/Brightness7';
import NavMenu from './NavMenu';
import { AppStoreContextItem } from '../Stores/AppStore';
import { observer } from 'mobx-react';

const drawerWidth = 240;
const miniDrawerWidth = 60;

// Define the props for Layout component
interface LayoutProps {
  children: React.ReactNode;
  toggleTheme: () => void;
  currentMode: 'light' | 'dark';
}

// Styled AppBar component
const StyledAppBar = styled(AppBar, {
  shouldForwardProp: (prop) => prop !== 'open' && prop !== 'isMobile',
})<{ open: boolean; isMobile: boolean }>(({ theme, open, isMobile }) => ({
  zIndex: theme.zIndex.drawer + 1,
  transition: theme.transitions.create(['width', 'margin'], {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  ...(open && !isMobile && {
    marginLeft: drawerWidth,
    width: `calc(100% - ${drawerWidth}px)`,
    transition: theme.transitions.create(['width', 'margin'], {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  }),
}));

// Drawer header to align items properly
const DrawerHeader = styled('div')(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'flex-end',
  padding: theme.spacing(0, 1),
  // necessary for content to be below AppBar
  ...theme.mixins.toolbar,
}));

// Main content area
const Main = styled('main', {
  shouldForwardProp: (prop) => prop !== 'open' && prop !== 'isMobile',
})<{ open: boolean; isMobile: boolean }>(({ theme, open, isMobile }) => ({
  flexGrow: 1,
  padding: theme.spacing(1), // Adds padding around the content
  transition: theme.transitions.create(['margin', 'width'], {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  marginLeft: isMobile ? 0 : open ? drawerWidth : miniDrawerWidth,
  boxSizing: 'border-box',
  overflow: 'auto', // Enables scrolling within Main
}));

const Layout: FC<LayoutProps> = observer(({ children, toggleTheme, currentMode }) => {
  const theme = useTheme();
  const isMobile = useMediaQuery((theme: Theme) => theme.breakpoints.down('sm'));
  const [open, setOpen] = useState(!isMobile);

  // Update drawer state when screen size changes
  useEffect(() => {
    setOpen(!isMobile);
  }, [isMobile]);

  const handleDrawerToggle = () => {
    setOpen(!open);
  };

  const store = AppStoreContextItem.useStore();
  const { isInitLoading } = store;

  if(isInitLoading) {
    return(<CircularProgress color="secondary" />);
  }

  return (
    <Box sx={{ display: 'flex', height: '100vh', overflow: 'hidden' }}> {/* Prevent outer scrollbar */}
      <CssBaseline />
      {/* AppBar */}
      <StyledAppBar position="fixed" open={open} isMobile={isMobile}>
        <Toolbar>
          {/* Show menu button on mobile or when Drawer is closed */}
          {(isMobile || !open) && (
            <IconButton
              color="inherit"
              aria-label={open ? 'close drawer' : 'open drawer'}
              edge="start"
              onClick={handleDrawerToggle}
              sx={{ marginRight: 0 }}
            >
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            Holiday Show Editor
          </Typography>
          {/* Theme Toggle Button */}
          <IconButton
            sx={{ ml: 1 }}
            onClick={toggleTheme}
            color="inherit"
            aria-label="toggle theme"
          >
            {currentMode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
          </IconButton>
        </Toolbar>
      </StyledAppBar>

      {/* Drawer */}
      <Drawer
        variant={isMobile ? 'temporary' : 'permanent'}
        open={open}
        onClose={handleDrawerToggle}
        ModalProps={{
          keepMounted: true, // Better open performance on mobile.
        }}
        sx={{
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: open ? drawerWidth : miniDrawerWidth,
            transition: theme.transitions.create('width', {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.enteringScreen,
            }),
            overflowX: 'hidden',
            boxSizing: 'border-box',
          },
        }}
      >
        <DrawerHeader>
          {/* Show close button only on permanent drawer when open */}
          {!isMobile && open && (
            <IconButton onClick={handleDrawerToggle}>
              <ChevronLeftIcon />
            </IconButton>
          )}
        </DrawerHeader>
        {/* Navigation Menu */}
        <NavMenu open={open} />
      </Drawer>

      {/* Main Content */}
      <Main open={open} isMobile={isMobile}>
        <Toolbar /> {/* Ensures content starts below AppBar */}
        {children}
      </Main>
    </Box>
  );
});

export default Layout;
