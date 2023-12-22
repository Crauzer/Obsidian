import RadixDialog from '@radix-ui/react-dialog';
import React from 'react';

export type DialogRootProps = RadixDialog.DialogProps;

export const DialogRoot: React.FC<DialogRootProps> = (props) => {
  return <RadixDialog.Root {...props} />;
};
