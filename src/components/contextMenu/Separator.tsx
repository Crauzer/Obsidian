import * as RadixContextMenu from '@radix-ui/react-context-menu';
import React from 'react';

export const ContextMenuSeparator: React.FC = () => {
  return <RadixContextMenu.Separator className="mx-1 my-1 h-[1px] bg-gray-500" />;
};
