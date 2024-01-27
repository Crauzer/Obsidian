import { ContextMenuContent } from './Content';
import { ContextMenuItem } from './Item';
import { ContextMenuRoot } from './Root';
import { ContextMenuSeparator } from './Separator';
import { ContextMenuTrigger } from './Trigger';

export * from './Root';
export * from './Trigger';
export * from './Content';
export * from './Item';
export * from './Separator';

export const ContextMenu = {
  Root: ContextMenuRoot,
  Trigger: ContextMenuTrigger,
  Content: ContextMenuContent,
  Item: ContextMenuItem,
  Separator: ContextMenuSeparator,
};
