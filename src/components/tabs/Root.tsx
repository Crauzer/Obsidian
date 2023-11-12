import * as RadixTabs from '@radix-ui/react-tabs';

export type TabsRootProps = RadixTabs.TabsProps;

export const TabsRoot: React.FC<TabsRootProps> = (props) => {
  return <RadixTabs.Root {...props}>{props.children}</RadixTabs.Root>;
};
