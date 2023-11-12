import * as RadixMenu from '@radix-ui/react-dropdown-menu';

type MenuTriggerProps = RadixMenu.DropdownMenuTriggerProps;

export const MenuTrigger: React.FC<MenuTriggerProps> = ({ children, ...props }) => {
  return <RadixMenu.Trigger {...props}>{children}</RadixMenu.Trigger>;
};
