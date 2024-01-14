import * as RadixContextMenu from '@radix-ui/react-context-menu';
import clsx from 'clsx';
import React from 'react';

export const ContextMenuContent: React.FC<RadixContextMenu.MenuContentProps> = (props) => (
  <RadixContextMenu.Portal>
    <RadixContextMenu.Content
      {...props}
      className={clsx(
        props.className,
        'flex min-w-[250px] flex-col gap-y-1 rounded-lg border border-gray-500 bg-gray-700 p-2 shadow-xl',
        'will-change-[opacity,transform]',

        'data-[state=open]:data-[side=top]:animate-slideAndFadeInFromTop',
        'data-[state=open]:data-[side=right]:animate-slideAndFadeInFromRight',
        'data-[state=open]:data-[side=bottom]:animate-slideAndFadeInFromBottom',
        'data-[state=open]:data-[side=left]:animate-slideAndFadeInFromLeft',

        'data-[state=closed]:data-[side=top]:animate-slideAndFadeOutFromTop',
        'data-[state=closed]:data-[side=right]:animate-slideAndFadeOutFromRight',
        'data-[state=closed]:data-[side=bottom]:animate-slideAndFadeOutFromBottom',
        'data-[state=closed]:data-[side=left]:animate-slideAndFadeOutFromLeft',
      )}
    />
  </RadixContextMenu.Portal>
);
