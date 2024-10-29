// src/components/ControlButton.tsx

import React from 'react';
import { Button, ButtonProps } from '@mui/material';

interface ControlButtonProps {
  onClick: () => void;
  color: ButtonProps['color'];
  text: string;
}

const ControlButton: React.FC<ControlButtonProps> = React.memo(({ onClick, color, text }) => {
  return (
    <Button
      variant="contained"
      color={color}
      onClick={onClick}
      size="large"
      fullWidth
      sx={{
        height: '60px', // Taller buttons for easier tapping
        textTransform: 'none', // Preserve the case of the button text
        fontSize: '1rem', // Ensure readability
      }}
    >
      {text}
    </Button>
  );
});

export default ControlButton;
