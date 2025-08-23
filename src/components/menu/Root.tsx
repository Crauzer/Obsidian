import * as RadixMenu from "@radix-ui/react-dropdown-menu";
import type React from "react";

export type MenuRootProps = RadixMenu.DropdownMenuProps;

export const MenuRoot: React.FC<MenuRootProps> = (props) => {
  return <RadixMenu.Root {...props} />;
};
